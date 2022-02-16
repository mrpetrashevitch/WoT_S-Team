#pragma once
#include "player.h"
#include "vehicle.h"
#include "action.h"

namespace ai
{
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
		void get_tanks_in_order(int curr_player,
			vehicles_native* vehicles, int vehicles_size,
			vehicles_native** result, int* result_size);
		action check_for_shooting(int curr_player,
			vehicles_native* vehicles, int vehicles_size,
			vehicles_native* vehicle,
			attack_matrix_native* attack_matrix, int attack_matrix_size);
		bool check_the_shooting_zone(vehicles_native* shooter, vehicles_native* goal);
		action move_tank(vehicles_native* vehicles, int vehicles_size, vehicles_native* vehicle);
		int distance(point a, point b);
		int get_destruct_points(vehicle_type type);
		int get_speed(vehicle_type type);
		bool check_neutrality(int curr_player, int goal,
			attack_matrix_native* attack_matrix, int attack_matrix_size);
	};
}