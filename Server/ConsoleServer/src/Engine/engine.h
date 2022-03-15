#pragma once

#include "../Thread/thread.h"

#include "../Packets/packets.h"
#include "battle.h"

#include <vector>
#include <map>

namespace engine
{

	class engine
	{
		bool _exit = false;
		void _loop();
		thread::thread _thread;

		web::io_server::i_server* _server = nullptr;

		std::mutex _mut;
		std::vector<std::shared_ptr<battle>> _battles;
		std::map<std::string, std::shared_ptr<battle>> _name_battle;
		std::map<web::io_base::i_connection*, std::string> _con_name;

		int _user_id = 1;

		std::shared_ptr<battle> _get_battle_by_conn(web::io_base::i_connection* conn);
		std::shared_ptr<battle> _get_battle_by_name(const std::string& name);

	public:
		engine();
		bool run(web::io_server::i_server* server);

		std::tuple<models::result, models::player> login(const models::login& login, web::io_base::i_connection* conn);
		std::tuple<models::result, models::map> map(web::io_base::i_connection* conn);
		std::tuple<models::result, models::game_state> game_state(web::io_base::i_connection* conn);
		std::tuple<models::result, models::action_rsp> actions(web::io_base::i_connection* conn);
		models::result move(const models::move& move, web::io_base::i_connection* conn);
		models::result shoot(const models::shoot& shoot, web::io_base::i_connection* conn);
		models::result turn(web::io_base::i_connection* conn_id);
	};

}
