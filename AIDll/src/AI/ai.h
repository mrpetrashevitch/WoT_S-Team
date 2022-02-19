#pragma once

#include <vector>
#include "player.h"
#include "vehicle.h"
#include "action.h"

namespace ai
{
	const int DIRECTIONS_NUMBER = 6;

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

		~ai() {
			if(positions != NULL)
				delete[] positions;
		}

	private:
		void get_tanks_in_order(int curr_player,
			vehicles_native* vehicles, int vehicles_size,
			vehicles_native** result, int* result_size);
		action check_for_shooting(int curr_player,
			vehicles_native* vehicles, int vehicles_size,
			vehicles_native* vehicle,
			attack_matrix_native* attack_matrix, int attack_matrix_size);
		bool check_the_shooting_zone(vehicles_native* shooter, vehicles_native* goal);
		action move_tank(vehicles_native* vehicles, int vehicles_size, 
			vehicles_native** ally_vehicles, int* ally_vehicles_size,
			vehicles_native* vehicle,
			point* obstacle, int obstacle_size);
		int distance(point a, point b);
		int get_destruct_points(vehicle_type type);
		int get_speed(vehicle_type type);
		bool check_neutrality(int curr_player, int goal,
			attack_matrix_native* attack_matrix, int attack_matrix_size);

		void get_reachable_hexes(std::vector<std::vector<point>>& points, 
			point* obstacle, int obstacle_size, 
			vehicles_native* vehicles, int vehicles_size, 
			point start, int speed);


		point hex_direction_vectors[6] = {
			point{ +1, 0, -1 }, point{ +1, -1, 0 }, point{ 0, -1, +1},
				point{ -1, 0, +1 }, point{ -1, +1, 0 }, point{ 0, +1, -1 },
		};

		/*point hex_direction(int direction) {
			return hex_direction_vectors[direction];
		}*/

		point hex_add(point hex, point vec) {
			return point{ hex.x + vec.x, hex.y + vec.y, hex.z + vec.z };
		}

		point hex_neighbor(point hex, int direction) {
			return hex_add(hex, hex_direction_vectors[direction]);
		}

		bool point_is_in_array(point pt, point* arr, int arr_size) {
			for (int i = 0; i < arr_size; i++) {
				if (pt.x == arr[i].x && pt.y == arr[i].y && pt.z == arr[i].z)
					return true;
			}
			return false;
		}

		void set_vehicles_positions(vehicles_native** vehicles, int* vehicles_size,
			point* obstacle, int obstacle_size);

		point* positions = NULL;
	};
}