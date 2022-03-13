#pragma once
#include <string>
#include <vector>
#include <map>

namespace models
{
	enum Action
	{
		LOGIN = 1,
		LOGOUT = 2,
		MAP = 3,
		GAME_STATE = 4,
		GAME_ACTIONS = 5,
		TURN = 6,
		CHAT = 100,
		MOVE = 101,
		SHOOT = 102,
	};

	enum Result
	{
		OKEY = 0,
		BAD_COMMAND = 1,
		ACCESS_DENIED = 2,
		INAPPROPRIATE_GAME_STATE = 3,
		TIMEOUT = 4,
		INTERNAL_SERVER_ERROR = 500,
	};

	struct player
	{
		int idx = 0;
		std::string name = "";
		bool is_observer = false;
	};

	struct login
	{
		std::string name = "";
		std::string password = "";
		std::string game = "";
		int num_turns = 0;
		int num_players = 0;
		bool is_observer = false;
	};

	struct login_response
	{
		int idx = 0;
		std::string name = "";
		bool is_observer = false;
	};

	struct point3
	{
		int x = 0, y = 0, z = 0;
	};

	struct content
	{
		std::vector<point3> base;
		std::vector<point3> obstacle;
		std::vector<point3> light_repair;
		std::vector<point3> hard_repair;
		std::vector<point3> catapult;
	};

	struct spawn_points
	{
		std::vector<point3> medium_tank;
		std::vector<point3> light_tank;
		std::vector<point3> heavy_tank;
		std::vector<point3> at_spg;
		std::vector<point3> spg;
	};

	struct map
	{
		int size = 0;
		std::string name = "";
		std::vector <spawn_points> spawn_points;
		content content;
	};

	enum vehicle_type : int
	{
		medium_tank,
		light_tank,
		heavy_tank,
		at_spg,
		spg
	};

	struct vehicle
	{
		int player_id = 0;
		vehicle_type vehicle_type = vehicle_type::medium_tank;
		int health = 0;
		point3 spawn_position{};
		point3 position{};
		int capture_points = 0;
		int shoot_range_bonus = 0;
	};

	struct win_points
	{
		int capture = 0;
		int kill = 0;
	};

	struct game_state
	{
		int num_players = 0;
		int num_turns = 0;
		int current_turn = 0;
		std::vector<player> players;
		std::vector<player> observers;
		int current_player_idx = 0; //?
		bool finished = false;
		std::map<int, vehicle> vehicles;
		std::map<int, std::vector<int>> attack_matrix;
		int winner = 0;//?
		std::map<int, win_points> win_points;
		std::vector<point3> catapult_usage;
	};
}