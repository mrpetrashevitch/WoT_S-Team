#include "engine.h"

namespace engine
{
	void engine::_loop()
	{

	}

	std::shared_ptr<battle> engine::_get_battle_by_conn_id(int conn_id)
	{
		auto iter_c = _con_name.find(conn_id);
		if (iter_c == _con_name.end())
			return nullptr;

		auto iter_n = _name_battle.find(iter_c->second);
		if (iter_n == _name_battle.end())
			return nullptr;

		return iter_n->second;
	}

	engine::engine()
	{
		std::unique_ptr<thread::thread> _worker_thread(std::make_unique<thread::thread>());
		_thread.set_func(std::bind(&engine::_loop, this));
		_thread.set_exit([this]
			{
				//PostQueuedCompletionStatus(_iocp, 0, 1, nullptr);
			}
		);

	}

	bool engine::run()
	{
		_thread.run();
		return true;
	}

	std::tuple<models::result, models::player> engine::login(const models::login& login, int conn_id)
	{
		auto iter_c = _con_name.find(conn_id);
		auto iter_n = _name_battle.find(login.name);

		if (iter_c != _con_name.end() && iter_n != _name_battle.end())
		{
			auto battle = iter_n->second;
			auto player = battle->get_player_by_name(login.name);
			return { models::result::OKEY, player };
		}

		if (iter_n != _name_battle.end())
		{
			auto battle = iter_n->second;
			auto player = battle->get_player_by_name(login.name);
			_con_name.insert(std::pair<int, std::string>(conn_id, login.name));
			return { models::result::OKEY, player };
		}

		if (iter_c == _con_name.end())
			_con_name.insert(std::pair<int, std::string>(conn_id, login.name));

		//create battle
		auto ptr_battle = std::make_shared<battle>(login.game, login.num_players, login.num_turns);

		//create player
		models::player player;
		player.name = login.name;
		player.is_observer = login.is_observer;
		player.idx = _user_id++;

		ptr_battle->add_player(player);
		_battles.push_back(ptr_battle);
		_name_battle.insert(std::pair<std::string, std::shared_ptr<battle>>(login.name, ptr_battle));

		return { models::result::OKEY, player };
	}

	std::tuple<models::result, models::map> engine::map(int conn_id)
	{
		auto battle = _get_battle_by_conn_id(conn_id);
		if (battle == nullptr) return { models::result::ACCESS_DENIED, {} };
		return { models::result::OKEY, battle->get_map()};
	}

	std::tuple<models::result, models::game_state> engine::game_state(int conn_id)
	{
		auto battle = _get_battle_by_conn_id(conn_id);
		if (battle == nullptr) return { models::result::ACCESS_DENIED, {} };
		return { models::result::OKEY, battle->get_game_state() };
	}

	std::tuple<models::result, models::action_rsp> engine::actions(int conn_id)
	{
		auto battle = _get_battle_by_conn_id(conn_id);
		if (battle == nullptr) return { models::result::ACCESS_DENIED, {} };
		return { models::result::OKEY, battle->get_actions() };
	}
}