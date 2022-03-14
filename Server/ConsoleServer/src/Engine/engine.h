#pragma once
#include "../Thread/thread.h"

#include "battle.h"

#include <vector>
#include <map>
#include <set>
namespace engine
{

	class engine
	{
		void _loop();
		thread::thread _thread;

		std::vector<std::shared_ptr<battle>> _battles;

		std::map<std::string, std::shared_ptr<battle>> _name_battle;
		std::map<int, std::string> _con_name;
		int _user_id = 1;

		std::shared_ptr<battle> _get_battle_by_conn_id(int conn_id);
	public:
		engine();
		bool run();

		std::tuple<models::result, models::player> login(const models::login& login, int conn_id);
		std::tuple<models::result, models::map> map(int conn_id);
		std::tuple<models::result, models::game_state> game_state(int conn_id);
		std::tuple<models::result, models::action_rsp> actions(int conn_id);
	};

}
