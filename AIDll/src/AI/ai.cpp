#include "ai.h"
#include <cmath>
#include <algorithm>
#include <fstream>
#include <vector>

namespace ai
{
	result ai::get_action(int curr_player,
		player_native* players, int players_size,
		vehicles_native* vehicles, int vehicles_size,
		win_points_native* win_points, int win_points_size,
		attack_matrix_native* attack_matrix, int attack_matrix_size,
		point* base, int base_size, 
		point* obstacle, int obstacle_size,
		action_ret* out_actions)
	{

		vehicles_native** tanks = new vehicles_native * [5];
		int tanks_size = 5;
		get_tanks_in_order(curr_player, vehicles, vehicles_size, tanks, &tanks_size);
		for (int i = 0; i < tanks_size; i++) {
			auto act = check_for_shooting(curr_player, vehicles, vehicles_size, tanks[i],
				attack_matrix, attack_matrix_size);
			out_actions->actions[i] = act;
			if (act.action_type == action_type::shoot) {
				continue;
			}
			act = move_tank(vehicles, vehicles_size, tanks[i]);
			out_actions->actions[i] = act;
		}

		return result::OKEY;
	}

	void ai::get_tanks_in_order(int curr_player,
		vehicles_native* vehicles, int vehicles_size,
		vehicles_native** result, int* result_size)
	{

		for (int i = 0; i < vehicles_size; i++) {
			if (vehicles[i].player_id != curr_player) {
				continue;
			}
			if (vehicles[i].vehicle_type == vehicle_type::SPG) {
				result[0] = &vehicles[i];
			}
			else if (vehicles[i].vehicle_type == vehicle_type::LT) {
				result[1] = &vehicles[i];
			}
			else if (vehicles[i].vehicle_type == vehicle_type::HT) {
				result[2] = &vehicles[i];
			}
			else if (vehicles[i].vehicle_type == vehicle_type::MT) {
				result[3] = &vehicles[i];
			}
			else if (vehicles[i].vehicle_type == vehicle_type::ASPG) {
				result[4] = &vehicles[i];
			}
		}
	}

	action ai::check_for_shooting(int curr_player,
		vehicles_native* vehicles, int vehicles_size,
		vehicles_native* vehicle,
		attack_matrix_native* attack_matrix, int attack_matrix_size)
	{

		action to_return;
		to_return.action_type = action_type::nun;

		if (vehicle->vehicle_type != vehicle_type::ASPG) {

			std::vector<vehicles_native*> targets;
			for (int i = 0; i < vehicles_size; i++) {
				if (check_the_shooting_zone(vehicle, &vehicles[i]) &&
					check_neutrality(curr_player, vehicles[i].player_id, attack_matrix, attack_matrix_size)) {
					targets.push_back(&vehicles[i]);
				}
			}

			std::sort(targets.begin(), targets.end(), [](vehicles_native* l, vehicles_native* r)
				{
					return l->health < r->health ||
						l->health == r->health && l->capture_points > r->capture_points;
				});

			if (!targets.empty()) {
				to_return.action_type = action_type::shoot;
				to_return.vec_id = vehicle->vehicle_id;
				to_return.point = targets[0]->position;
				targets[0]->health--;
			}

		}
		else {

			struct direction_parameters
			{
				point target;
				int sum_capture_points;
				int sum_destruct_points;
				int sum_health;
				std::vector<vehicles_native*> vehicles;
				direction_parameters(point target) : target(target), sum_capture_points(0), sum_destruct_points(0),
					sum_health(0), vehicles() {}
			};

			std::vector<direction_parameters> params;
			point current_position = vehicle->position;
			params.push_back(direction_parameters(
				point{ current_position.x, current_position.y + 1, current_position.z - 1 })); // for x forwarding up
			params.push_back(direction_parameters(
				point{ current_position.x, current_position.y - 1, current_position.z + 1 })); // for x forwarding down
			params.push_back(direction_parameters(
				point{ current_position.x + 1, current_position.y, current_position.z - 1 })); // for y forwarding up
			params.push_back(direction_parameters(
				point{ current_position.x - 1, current_position.y, current_position.z + 1 })); // for y forwarding down
			params.push_back(direction_parameters(
				point{ current_position.x - 1, current_position.y + 1, current_position.z })); // for z forwarding up
			params.push_back(direction_parameters(
				point{ current_position.x + 1, current_position.y - 1, current_position.z })); // for z forwarding down

			for (int i = 0; i < vehicles_size; i++) {
				if (curr_player == vehicles[i].player_id ||
					!check_neutrality(curr_player, vehicles[i].player_id, attack_matrix, attack_matrix_size) ||
					vehicles[i].health == 0) {
					continue;
				}
				int dist = distance(vehicle->position, vehicles[i].position);
				if (dist <= 3) {
					if (vehicle->position.x == vehicles[i].position.x) {
						if (vehicle->position.y < vehicles[i].position.y) {
							params[0].sum_capture_points += vehicles[i].capture_points;
							if (vehicles[i].health == 1) {
								params[0].sum_destruct_points += get_destruct_points(vehicles[i].vehicle_type);
							}
							params[0].sum_health++;
							params[0].vehicles.push_back(&vehicles[i]);
						}
						else {
							params[1].sum_capture_points += vehicles[i].capture_points;
							if (vehicles[i].health == 1) {
								params[1].sum_destruct_points += get_destruct_points(vehicles[i].vehicle_type);
							}
							params[1].sum_health++;
							params[1].vehicles.push_back(&vehicles[i]);
						}
					}
					if (vehicle->position.y == vehicles[i].position.y) {
						if (vehicle->position.x < vehicles[i].position.x) {
							params[2].sum_capture_points += vehicles[i].capture_points;
							if (vehicles[i].health == 1) {
								params[2].sum_destruct_points += get_destruct_points(vehicles[i].vehicle_type);
							}
							params[2].sum_health++;
							params[2].vehicles.push_back(&vehicles[i]);
						}
						else {
							params[3].sum_capture_points += vehicles[i].capture_points;
							if (vehicles[i].health == 1) {
								params[3].sum_destruct_points += get_destruct_points(vehicles[i].vehicle_type);
							}
							params[3].sum_health++;
							params[3].vehicles.push_back(&vehicles[i]);
						}
					}
					if (vehicle->position.z == vehicles[i].position.z) {
						if (vehicle->position.x > vehicles[i].position.x) {
							params[4].sum_capture_points += vehicles[i].capture_points;
							if (vehicles[i].health == 1) {
								params[4].sum_destruct_points += get_destruct_points(vehicles[i].vehicle_type);
							}
							params[4].sum_health++;
							params[4].vehicles.push_back(&vehicles[i]);
						}
						else {
							params[5].sum_capture_points += vehicles[i].capture_points;
							if (vehicles[i].health == 1) {
								params[5].sum_destruct_points += get_destruct_points(vehicles[i].vehicle_type);
							}
							params[5].sum_health++;
							params[5].vehicles.push_back(&vehicles[i]);
						}
					}
				}
			}

			std::sort(params.begin(), params.end(), [](direction_parameters l, direction_parameters r) {
				return l.sum_capture_points > r.sum_capture_points ||
					l.sum_capture_points == r.sum_capture_points && l.sum_destruct_points > r.sum_destruct_points ||
					l.sum_capture_points == r.sum_capture_points && l.sum_destruct_points == r.sum_destruct_points &&
					l.sum_health > r.sum_health;
				});

			if (params[0].sum_health != 0) {
				to_return.action_type = action_type::shoot;
				to_return.vec_id = vehicle->vehicle_id;
				to_return.point = params[0].target;
				for (auto& veh : params[0].vehicles) {
					veh->health--;
				}
			}

		}

		return to_return;

	}

	action ai::move_tank(vehicles_native* vehicles, int vehicles_size, vehicles_native* vehicle)
	{
		point base_center = point();
		int min_value = distance(vehicle->position, base_center);
		int speed = get_speed(vehicle->vehicle_type);
		action to_return;
		to_return.action_type = action_type::move;
		to_return.vec_id = vehicle->vehicle_id;
		to_return.point = vehicle->position;
		for (int dx = -speed; dx <= speed; dx++) {
			for (int dy = -speed; dy <= speed; dy++) {
				int dz = 0 - dx - dy;
				point new_point = vehicle->position;
				new_point.x += dx;
				new_point.y += dy;
				new_point.z += dz;
				int dist0 = distance(vehicle->position, new_point);
				if (dist0 > speed ||
					dist0 == 0) {
					continue;
				}
				int dist_to_base = distance(new_point, base_center);
				if (dist_to_base < min_value) {
					bool position_ok = true;
					for (int t = 0; t < vehicles_size; t++) {
						if (distance(vehicles[t].position, new_point) == 0) {
							position_ok = false;
							break;
						}
					}
					if (!position_ok) {
						continue;
					}
					to_return.point = new_point;
					min_value = dist_to_base;
				}
			}
		}
		return to_return;
	}

	int ai::get_destruct_points(vehicle_type type)
	{
		if (type == vehicle_type::SPG || type == vehicle_type::LT) {
			return 1;
		}
		if (type == vehicle_type::MT || type == vehicle_type::ASPG) {
			return 2;
		}
		if (type == vehicle_type::HT) {
			return 3;
		}
		throw std::invalid_argument("vehicle_type error");
	}

	int ai::get_speed(vehicle_type type)
	{
		if (type == vehicle_type::SPG || type == vehicle_type::HT || type == vehicle_type::ASPG) {
			return 1;
		}
		if (type == vehicle_type::MT) {
			return 2;
		}
		if (type == vehicle_type::LT) {
			return 3;
		}
		throw std::invalid_argument("vehicle_type error");
	}

	bool ai::check_the_shooting_zone(vehicles_native* shooter, vehicles_native* goal)
	{
		if (goal->health == 0 || shooter->player_id == goal->player_id) {
			return false;
		}

		if (shooter->vehicle_type == vehicle_type::SPG) {
			return distance(shooter->position, goal->position) == 3;
		}
		if (shooter->vehicle_type == vehicle_type::LT || shooter->vehicle_type == vehicle_type::MT) {
			return distance(shooter->position, goal->position) == 2;
		}
		if (shooter->vehicle_type == vehicle_type::HT) {
			return distance(shooter->position, goal->position) > 0 && distance(shooter->position, goal->position) <= 2;
		}
		throw std::invalid_argument("vehicle_type error");
	}

	int ai::distance(point a, point b)
	{
		return (abs(a.x - b.x) + abs(a.y - b.y) + abs(a.z - b.z)) / 2;
	}

	bool ai::check_neutrality(int curr_player, int goal,
		attack_matrix_native* attack_matrix, int attack_matrix_size)
	{
		for (int i = 0; i < attack_matrix_size; i++) {
			if (attack_matrix[i].id != goal) {
				continue;
			}
			for (int j = 0; j < 3; j++) {
				if (attack_matrix[i].attack[j] == curr_player) {
					return true;
				}
			}
		}
		bool was_attacked_by_third = false;
		for (int i = 0; i < attack_matrix_size; i++) {
			for (int j = 0; j < 3; j++) {
				if (attack_matrix[i].attack[j] == goal && attack_matrix[i].id != curr_player) {
					was_attacked_by_third = true;
				}
			}
		}
		return !was_attacked_by_third;
	}
}