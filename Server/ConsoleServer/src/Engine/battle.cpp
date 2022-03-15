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

	models::player battle::_get_next_player()
	{
		auto iter = std::find_if(_game_state.players.begin(), _game_state.players.end(),
			[this](const models::player& p) -> bool { return p.idx == _game_state.current_player_idx; });
		iter++;
		if (iter == _game_state.players.end())
			return _game_state.players[0];
		return *iter;
	}

	int battle::_get_player_index(web::io_base::i_connection* conn)
	{
		auto iter = std::find(_player_conn.begin(), _player_conn.end(), conn);
		if (iter == _player_conn.end())
			return -1;
		return iter - _player_conn.begin();
	}

	bool battle::_vehicle_is_eat(int veh_id)
	{
		auto iter = _game_state.vehicles.find(veh_id);
		if (iter == _game_state.vehicles.end())
			return false;
		return true;
	}

	std::vector<models::point3> battle::_get_point_around(const models::point3& point, int n, int N)
	{
		std::vector<models::point3> res;
		if (n > N || n < 0 || N < 0) return {};

		models::point3 hex{};
		for (int dx = -N; dx <= N; dx++)
		{
			hex.x = dx + point.x;
			if (std::abs(hex.x) > _map.size - 1) continue;

			for (int dy = -N; dy <= N; dy++)
			{
				hex.y = dy + point.y;
				if (std::abs(hex.y) > _map.size - 1) continue;

				for (int dz = -N; dz <= N; dz++)
				{
					hex.z = dz + point.z;
					if (std::abs(hex.z) > _map.size - 1) continue;
					if (dx + dy + dz != 0) continue;

					int dist = models::point3::get_distance(point, hex);
					if (dist < n || dist > N) continue;
					res.push_back(hex);
				}
			}
		}
		return res;
	}

	std::vector<models::point3> battle::_get_points_move(const models::point3& point, int speed)
	{
		std::vector<models::point3> set;
		std::vector<models::point3> list;
		list.push_back(point);

		for (int i = 1; i <= speed; i++)
		{
			std::vector<models::point3> temp;
			for (auto& item : list)
			{
				std::vector<models::point3> l = _get_point_around(item, 1, 1);

				l.erase(std::remove_if(l.begin(), l.end(),
					[this](const models::point3& p)
					{
						return std::find(_map.content.obstacle.begin(), _map.content.obstacle.end(), p) != _map.content.obstacle.end();
					}),
					l.end());

				for (auto& it : l)
				{
					if (std::find(set.begin(), set.end(), it) == set.end())
					{
						set.push_back(it);
						temp.push_back(it);
					}
				}
			}
			std::swap(list, temp);
		}

		set.erase(std::remove_if(set.begin(), set.end(),
			[this](const models::point3& p)
			{
				auto it = std::find_if(_game_state.vehicles.begin(), _game_state.vehicles.end(),
					[p](const std::pair<int, models::vehicle>& t) -> bool
					{
						return t.second.position == p;
					}
				);
				return it != _game_state.vehicles.end();
			}),
			set.end());
		return set;
	}

	bool battle::_get_vector(const models::point3& src, const models::point3& dst, int offset, models::point3& out)
	{
		bool valid = false;
		const int* ptr_p = (const int*)&src;
		const int* ptr_t = (const int*)&dst;
		int* ptr_n = (int*)&out;

		for (int i = 0; i < 3; i++)
		{
			int v_p = *(ptr_p + i);
			int v_t = *(ptr_t + i);
			if (v_p == v_t) { *(ptr_n + i) = v_p; valid = true; continue; }
			if (v_p > v_t) *(ptr_n + i) = v_p - offset;
			else *(ptr_n + i) = v_p + offset;
		}
		return valid;
	}

	std::vector<models::point3> battle::_get_path_shoot_pt(const models::point3& point, const models::point3& target, int shoot_max)
	{
		std::vector<models::point3> ret;
		for (size_t i = 1; i <= shoot_max; i++)
		{
			models::point3 p{};
			if (!_get_vector(point, target, i, p))
				break;
			if (std::find(_map.content.obstacle.begin(), _map.content.obstacle.end(), p) != _map.content.obstacle.end())
				break;
			ret.push_back(p);
		}
		return ret;
	}

	bool battle::_can_move(const models::vehicle& vehicle, const models::point3& point)
	{
		if (std::abs(point.x) > _map.size - 1) return false;
		if (std::abs(point.y) > _map.size - 1) return false;
		if (std::abs(point.z) > _map.size - 1) return false;

		int dist = models::point3::get_distance(vehicle.position, point);
		if (_vehicle_params[(int)vehicle.vehicle_type].speed < dist) return false;

		auto p = _get_points_move(vehicle.position, _vehicle_params[(int)vehicle.vehicle_type].speed);
		return std::find(p.begin(), p.end(), point) != p.end();
	}

	std::vector<models::point3> battle::_can_shoot(const models::vehicle& vehicle, const models::point3& point)
	{
		std::vector<models::point3> list;
		if (std::abs(point.x) > _map.size - 1) return list;
		if (std::abs(point.y) > _map.size - 1) return list;
		if (std::abs(point.z) > _map.size - 1) return list;

		int dist = models::point3::get_distance(vehicle.position, point);
		int shoot_min = _vehicle_params[(int)vehicle.vehicle_type].shoot_min;
		int shoot_max = _vehicle_params[(int)vehicle.vehicle_type].shoot_max;
		if (shoot_min > dist) return list;
		if (shoot_max < dist) return list;

		if (vehicle.vehicle_type == models::vehicle_type::at_spg)
			list = _get_path_shoot_pt(vehicle.position, point, shoot_max + vehicle.shoot_range_bonus);
		else
			list = _get_point_around(vehicle.position, shoot_min, shoot_max + vehicle.shoot_range_bonus);

		list.erase(std::remove_if(list.begin(), list.end(),
			[this, &vehicle](const models::point3& p)
			{
				auto it = std::find_if(_game_state.vehicles.begin(), _game_state.vehicles.end(),
					[p](const std::pair<int, models::vehicle>& v) -> bool
					{
						return v.second.position == p;
					}
				);
				if (it == _game_state.vehicles.end()) return true;
				if ((*it).second.player_id == vehicle.player_id) return true;
				return false;
			}),
			list.end());

		return list;
	}

	std::vector<post_action> battle::_next()
	{
		if (_game_state.finished) return {};
		if (_game_state.players.size() != _game_state.num_players) return {};

		if (_time.count() + TIME_STEP < std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::system_clock::now().time_since_epoch()).count() ||
			_waiters.size() == _game_state.players.size() + _game_state.observers.size())
		{
			if (_game_state.current_player_idx == 0)
				_game_state.current_player_idx = _game_state.players[0].idx;
			else
				_game_state.current_player_idx = _get_next_player().idx;

			std::vector<post_action> act;
			for (auto& i : _waiters)
				act.push_back({ i,models::result::OKEY,{} });
			_waiters.clear();


			//gev vehicle on base and add points for base

			_game_state.current_turn++;
			if (_game_state.current_turn == _game_state.num_turns)
			{
				// get winner
				_game_state.finished = true;
			}
			_time = std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::system_clock::now().time_since_epoch());
			return act;
		}

		return{};
	}

	battle::battle(std::string name, int num_players, int num_turns) : _name(name), _time(std::chrono::milliseconds(0))
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

	bool battle::add_player(const models::player& player, web::io_base::i_connection* conn)
	{
		if (player.is_observer)
		{
			_game_state.observers.push_back(player);
			_observer_conn.push_back(conn);
			return true;
		}

		if (_game_state.num_players == _game_state.players.size()) return false;
		_game_state.players.push_back(player);
		_player_conn.push_back(conn);
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

	const std::string& battle::get_name()
	{
		return _name;
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

	models::result battle::set_turn(web::io_base::i_connection* conn)
	{
		_waiters.push_back(conn);
		return models::result::OKEY;
	}

	models::result battle::set_move(const models::move& move, web::io_base::i_connection* conn)
	{
		int index_player = _get_player_index(conn);
		if (index_player == -1) return models::result::ACCESS_DENIED;
		if (!_vehicle_is_eat(move.vehicle_id)) return models::result::BAD_COMMAND;
		auto& vehicle = _game_state.vehicles[move.vehicle_id];
		if (vehicle.player_id != _game_state.players[index_player].idx) return models::result::BAD_COMMAND;
		if (!_can_move(vehicle, move.target)) return models::result::BAD_COMMAND;

		vehicle.position = move.target;
		return models::result::OKEY;
	}

	models::result battle::set_shoot(const models::shoot& shoot, web::io_base::i_connection* conn)
	{
		int index_player = _get_player_index(conn);
		if (index_player == -1) return models::result::ACCESS_DENIED;
		if (!_vehicle_is_eat(shoot.vehicle_id)) return models::result::BAD_COMMAND;
		auto& vehicle = _game_state.vehicles[shoot.vehicle_id];
		if (vehicle.player_id != _game_state.players[index_player].idx) return models::result::BAD_COMMAND;
		auto points = _can_shoot(vehicle, shoot.target);

		// killing hevicles and add poinp kill and spawn vehicles
		if (vehicle.vehicle_type == models::vehicle_type::at_spg)
		{

		}
		else
		{

		}

		return models::result::OKEY;
	}

}