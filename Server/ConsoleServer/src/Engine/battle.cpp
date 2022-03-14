#include "battle.h"
namespace engine
{
	void battle::_init_map()
	{
		_map.name = "map04";
		_map.size = 11;

		_map.spawn_points.push_back({ {{ -4,-6,10 }},{{-6,-4,10}},{{-5,-5,10}},{{-3,-7,10}},{{-7,-3,10}} });
		_map.spawn_points.push_back({ {{ -6,10,-4 }},{{-4,10,-6}},{{-5,10,-5}},{{-7,10,-3}},{{-3,10,-7}} });
		_map.spawn_points.push_back({ {{ 10,-4,-6 }},{{10,-6,-4}},{{10,-5,-5}},{{10,-3,-7}},{{10,-7,-3}} });

		_map.content.base.push_back({ -1,0,1 });
		_map.content.base.push_back({ -1,1,0 });
		_map.content.base.push_back({ 0,-1,1 });
		_map.content.base.push_back({ 0,0,0 });
		_map.content.base.push_back({ 0,1,-1 });
		_map.content.base.push_back({ 1,-1,0 });
		_map.content.base.push_back({ 1,0,-1 });

		_map.content.catapult.push_back({ -6,3,3 });
		_map.content.catapult.push_back({ 3,-6,3 });
		_map.content.catapult.push_back({ 3,3,-6 });

		_map.content.hard_repair.push_back({ -2,-2,4 });
		_map.content.hard_repair.push_back({ -2,4,-2 });
		_map.content.hard_repair.push_back({ 4,-2,-2 });

		_map.content.light_repair.push_back({ -3,-3,6 });
		_map.content.light_repair.push_back({ -3,6,-3 });
		_map.content.light_repair.push_back({ 6,-3,-3 });

		_map.content.obstacle.push_back({ -10,  0,  10 });
		_map.content.obstacle.push_back({ -10,  10,  0 });
		_map.content.obstacle.push_back({ -9,  0,  9 });
		_map.content.obstacle.push_back({ -9,  9,  0 });
		_map.content.obstacle.push_back({ -8,  0,  8 });
		_map.content.obstacle.push_back({ -8,  8,  0 });
		_map.content.obstacle.push_back({ -7,  0,  7 });
		_map.content.obstacle.push_back({ -7,  7,  0 });
		_map.content.obstacle.push_back({ -6,  0,  6 });
		_map.content.obstacle.push_back({ -6,  6,  0 });
		_map.content.obstacle.push_back({ -5,  0,  5 });
		_map.content.obstacle.push_back({ -5,  5,  0 });
		_map.content.obstacle.push_back({ -3,  0,  3 });
		_map.content.obstacle.push_back({ -3,  3,  0 });
		_map.content.obstacle.push_back({ -2,  0,  2 });
		_map.content.obstacle.push_back({ -2,  2,  0 });
		_map.content.obstacle.push_back({ 0,  -10,  10 });
		_map.content.obstacle.push_back({ 0,  -9,  9 });
		_map.content.obstacle.push_back({ 0,  -8,  8 });
		_map.content.obstacle.push_back({ 0,  -7,  7 });
		_map.content.obstacle.push_back({ 0,  -6,  6 });
		_map.content.obstacle.push_back({ 0,  -5,  5 });
		_map.content.obstacle.push_back({ 0,  -3,  3 });
		_map.content.obstacle.push_back({ 0,  -2,  2 });
		_map.content.obstacle.push_back({ 0,  2,  -2 });
		_map.content.obstacle.push_back({ 0,  3,  -3 });
		_map.content.obstacle.push_back({ 0,  5,  -5 });
		_map.content.obstacle.push_back({ 0,  6,  -6 });
		_map.content.obstacle.push_back({ 0,  7,  -7 });
		_map.content.obstacle.push_back({ 0,  8,  -8 });
		_map.content.obstacle.push_back({ 0,  9,  -9 });
		_map.content.obstacle.push_back({ 0,  10,  -10 });
		_map.content.obstacle.push_back({ 2,  -2,  0 });
		_map.content.obstacle.push_back({ 2,  0,  -2 });
		_map.content.obstacle.push_back({ 3,  -3,  0 });
		_map.content.obstacle.push_back({ 3,  0,  -3 });
		_map.content.obstacle.push_back({ 5,  -5,  0 });
		_map.content.obstacle.push_back({ 5,  0,  -5 });
		_map.content.obstacle.push_back({ 6,  -6,  0 });
		_map.content.obstacle.push_back({ 6,  0,  -6 });
		_map.content.obstacle.push_back({ 7,  -7,  0 });
		_map.content.obstacle.push_back({ 7,  0,  -7 });
		_map.content.obstacle.push_back({ 8,  -8,  0 });
		_map.content.obstacle.push_back({ 8,  0,  -8 });
		_map.content.obstacle.push_back({ 9,  -9,  0 });
		_map.content.obstacle.push_back({ 9,  0,  -9 });
		_map.content.obstacle.push_back({ 10,  -10,  0 });
		_map.content.obstacle.push_back({ 10,  0,  -10 });
	}
	void battle::_init_game_state()
	{
		
	}
	models::vehicle battle::_create_vehicle(models::vehicle_type type, int player_index, const models::point3& pos)
	{
		models::vehicle v;
		v.vehicle_type = type;
		v.player_id = _game_state.players[player_index].idx;
		v.health = _vehicle_params[(int)type].hp_max;
		v.spawn_position = pos;
		v.position = pos;
		return v;
	}

	void battle::_add_vehicles(int player_index)
	{
		for (int i = 0; i < VEHICLE_TYPE_COUNT; i++)
		{
			auto& spawn_points = _map.spawn_points[player_index].get_spawn_points_by_type((models::vehicle_type)i);
			for (auto& var : spawn_points)
			{
				auto vehocle = _create_vehicle((models::vehicle_type)i, player_index, var);
				_game_state.vehicles.insert(std::pair<int, models::vehicle>(_vehicle_id++, vehocle));
			}
		}
	}

	battle::battle(std::string name, int num_players, int num_turns) : _name(name)
	{
		if (num_players < 1) num_players = 1;
		if (num_players > 3) num_players = 3;
		if (num_turns < 1) num_turns = 1;
		if (num_turns > 150) num_turns = 150;

		if (_name == "") num_players = 1;

		_game_state.num_players = num_players;
		_game_state.num_turns = num_turns;

		_init_map();
		_init_game_state();
	}

	bool battle::add_player(const models::player& player)
	{
		if (player.is_observer)
		{
			_game_state.observers.push_back(player);
			return true;
		}

		if (_game_state.num_players == _game_state.players.size()) return false;
		_game_state.players.push_back(player);
		_add_vehicles(_game_state.players.size() - 1);
		_game_state.attack_matrix.insert(std::pair<int, std::vector<int>>(player.idx, {}));
		_game_state.win_points.insert(std::pair<int, models::win_points>(player.idx, {}));
		return true;
	}

	models::player battle::get_player_by_name(const std::string name)
	{
		for (auto& i : _game_state.players)
			if (i.name == name)
				return i;

		for (auto& i : _game_state.observers)
			if (i.name == name)
				return i;
		return {};
	}

	const models::map& battle::get_map()
	{
		return _map;
	}

	const models::game_state& battle::get_game_state()
	{
		return _game_state;
	}

	const models::action_rsp& battle::get_actions()
	{
		return _actions;
	}


}