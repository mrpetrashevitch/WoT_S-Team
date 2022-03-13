#include "engine.h"

namespace engine
{
	void engine::_loop()
	{

	}

	void engine::_create_game()
	{
		models::game_state gs;

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

	std::tuple<models::Result, models::player> engine::login(const models::login& login, int conn_id)
	{
		models::player pl;

		if (_users.find(login.name) == _users.end())
			_users.insert(std::pair<std::string, int>(login.name, _user_id++));

		int user_id = _users[login.name];

		pl.idx = user_id;
		pl.is_observer = login.is_observer;
		pl.name = login.name;

		return { models::Result::OKEY, pl };
	}

	std::tuple<models::Result, models::map> engine::map(int conn_id)
	{
		models::map map;

		map.name = "map04";
		map.size = 11;
		map.spawn_points.push_back({ {{ -4,-6,10 }},{{-6,-4,10}},{{-5,-5,10}},{{-3,-7,10}},{{-7,-3,10}} });
		map.spawn_points.push_back({ {{ -6,10,-4 }},{{-4,10,-6}},{{-5,10,-5}},{{-7,10,-3}},{{-3,10,-7}} });
		map.spawn_points.push_back({ {{ 10,-4,-6 }},{{10,-6,-4}},{{10,-5,-5}},{{10,-3,-7}},{{10,-7,-3}} });

		map.content.base.push_back({ -1,0,1 });
		map.content.base.push_back({ -1,1,0 });
		map.content.base.push_back({ 0,-1,1 });
		map.content.base.push_back({ 0,0,0 });
		map.content.base.push_back({ 0,1,-1 });
		map.content.base.push_back({ 1,-1,0 });
		map.content.base.push_back({ 1,0,-1 });

		map.content.catapult.push_back({ -6,3,3 });
		map.content.catapult.push_back({ 3,-6,3 });
		map.content.catapult.push_back({ 3,3,-6 });

		map.content.hard_repair.push_back({ -6,-6,6 });
		map.content.hard_repair.push_back({ -6,6,-6 });
		map.content.hard_repair.push_back({ 6,-6,-6 });

		map.content.light_repair.push_back({});

		map.content.obstacle.push_back({ -10,  0,  10 });
		map.content.obstacle.push_back({ -10,  10,  0 });
		map.content.obstacle.push_back({ -9,  0,  9 });
		map.content.obstacle.push_back({ -9,  9,  0 });
		map.content.obstacle.push_back({ -8,  0,  8 });
		map.content.obstacle.push_back({ -8,  8,  0 });
		map.content.obstacle.push_back({ -7,  0,  7 });
		map.content.obstacle.push_back({ -7,  7,  0 });
		map.content.obstacle.push_back({ -6,  0,  6 });
		map.content.obstacle.push_back({ -6,  6,  0 });
		map.content.obstacle.push_back({ -5,  0,  5 });
		map.content.obstacle.push_back({ -5,  5,  0 });
		map.content.obstacle.push_back({ -3,  0,  3 });
		map.content.obstacle.push_back({ -3,  3,  0 });
		map.content.obstacle.push_back({ -2,  0,  2 });
		map.content.obstacle.push_back({ -2,  2,  0 });
		map.content.obstacle.push_back({ 0,  -10,  10 });
		map.content.obstacle.push_back({ 0,  -9,  9 });
		map.content.obstacle.push_back({ 0,  -8,  8 });
		map.content.obstacle.push_back({ 0,  -7,  7 });
		map.content.obstacle.push_back({ 0,  -6,  6 });
		map.content.obstacle.push_back({ 0,  -5,  5 });
		map.content.obstacle.push_back({ 0,  -3,  3 });
		map.content.obstacle.push_back({ 0,  -2,  2 });
		map.content.obstacle.push_back({ 0,  2,  -2 });
		map.content.obstacle.push_back({ 0,  3,  -3 });
		map.content.obstacle.push_back({ 0,  5,  -5 });
		map.content.obstacle.push_back({ 0,  6,  -6 });
		map.content.obstacle.push_back({ 0,  7,  -7 });
		map.content.obstacle.push_back({ 0,  8,  -8 });
		map.content.obstacle.push_back({ 0,  9,  -9 });
		map.content.obstacle.push_back({ 0,  10,  -10 });
		map.content.obstacle.push_back({ 2,  -2,  0 });
		map.content.obstacle.push_back({ 2,  0,  -2 });
		map.content.obstacle.push_back({ 3,  -3,  0 });
		map.content.obstacle.push_back({ 3,  0,  -3 });
		map.content.obstacle.push_back({ 5,  -5,  0 });
		map.content.obstacle.push_back({ 5,  0,  -5 });
		map.content.obstacle.push_back({ 6,  -6,  0 });
		map.content.obstacle.push_back({ 6,  0,  -6 });
		map.content.obstacle.push_back({ 7,  -7,  0 });
		map.content.obstacle.push_back({ 7,  0,  -7 });
		map.content.obstacle.push_back({ 8,  -8,  0 });
		map.content.obstacle.push_back({ 8,  0,  -8 });
		map.content.obstacle.push_back({ 9,  -9,  0 });
		map.content.obstacle.push_back({ 9,  0,  -9 });
		map.content.obstacle.push_back({ 10,  -10,  0 });
		map.content.obstacle.push_back({ 10,  0,  -10 });


		return { models::Result::OKEY, map };
	}


}