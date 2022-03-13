#pragma once
#include "../Thread/thread.h"
#include "../Model/model.h"

#include <map>
#include <set>
namespace engine
{
	struct user
	{
		int conn_id;
		int game_id;
	};


	class engine
	{
		void _loop();
		thread::thread _thread;


		std::map<std::string, int> _users;
		int _user_id = 1;
		void _create_game();
	public:
		engine();
		bool run();

		std::tuple<models::Result, models::player> login(const models::login& login, int conn_id);
		std::tuple<models::Result, models::map> map(int conn_id);
	};

}
