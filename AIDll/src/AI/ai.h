#pragma once

#include <array>
#include <vector>
#include "player.h"
#include "vehicle.h"
#include "action.h"

namespace ai
{
	const int DIRECTIONS_NUMBER = 6;
	const int ALLY_TANKS_NUMBER = 5;
	const int SPG_POSITION_INDEX = 0;
	const int LT_POSITION_INDEX = 1;
	const int HT_POSITION_INDEX = 2;
	const int MT_POSITION_INDEX = 3;
	const int ASPG_POSITION_INDEX = 4;

	class ai
	{
	public:
		result get_action(int curr_player,
			player_native* players, int players_size,
			vehicles_native* vehicles, int vehicles_size,
			win_points_native* win_points, int win_points_size,
			attack_matrix_native* attack_matrix, int attack_matrix_size,
			point* base, int base_size,
			point* obstacle, int obstacle_size,
			action_ret* out_actions);

	private:
		void get_tanks_in_order(int& curr_player, std::vector<vehicles_native>& vehicles,
			std::vector<vehicles_native>& result);
		void set_vehicles_positions(std::vector<vehicles_native>& vehicles, std::vector<point>& obstacles);
		action check_for_shooting(int& curr_player, std::vector<vehicles_native>& vehicles,
			vehicles_native& vehicle, std::vector<attack_matrix_native>& attack_matrix);
		bool check_the_shooting_zone(vehicles_native& shooter, vehicles_native& goal);
		action move_tank(std::vector<vehicles_native>& vehicles, std::vector<vehicles_native>& ally_vehicles,
			vehicles_native& vehicle, std::vector<point>& obstacles);
		int distance(point& a, point& b);
		int get_destruct_points(vehicle_type& type);
		int get_speed(vehicle_type& type);
		bool check_neutrality(int& curr_player, int& goal, std::vector<attack_matrix_native>& attack_matrix);
		void get_reachable_hexes(std::vector<std::vector<point>>& points, std::vector<point>& obstacles,
			std::vector<vehicles_native>& vehicles, point& start, int& speed);
		point get_hex_sum(const point& hex, const point& vec);
		point get_hex_neighbor(point& hex, int& direction);
		bool point_is_in_array(point& pt, std::vector<point>& arr);

	private:
		const std::array<point, DIRECTIONS_NUMBER> hex_direction_vectors = {
			point{ +1, 0, -1 }, point{ +1, -1, 0 }, point{ 0, -1, +1},
			point{ -1, 0, +1 }, point{ -1, +1, 0 }, point{ 0, +1, -1 },
		};
		std::vector<point> key_positions;
	};
}