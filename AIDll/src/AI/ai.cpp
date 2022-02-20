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

		if (positions == NULL) {
			positions = new point[tanks_size];
			for (int i = 0; i < tanks_size; i++) {
				point pos = tanks[i]->spawn_position;
				positions[i] = tanks[i]->spawn_position;
			}

			set_vehicles_positions(tanks, &tanks_size, obstacle, obstacle_size);
		}

		for (int i = 0; i < tanks_size; i++) {
			auto act = check_for_shooting(curr_player, vehicles, vehicles_size, tanks[i],
				attack_matrix, attack_matrix_size);
			out_actions->actions[i] = act;
			if (act.action_type == action_type::shoot) {
				continue;
			}
			act = move_tank(vehicles, vehicles_size, 
				tanks, &tanks_size, 
				tanks[i], 
				obstacle, obstacle_size);
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

	void ai::get_reachable_hexes(std::vector<std::vector<point>>& points,
		point* obstacle, int obstacle_size,
		vehicles_native* vehicles, int vehicles_size,
		point start, int speed) {

		std::vector<point> visited;
		visited.push_back(start);
		points.push_back(std::vector<point>());
		points[0].push_back(start);

		point *vehicles_positions = new point[vehicles_size];
		for (int i = 0; i < vehicles_size; i++)
			vehicles_positions[i] = vehicles[i].position;

		for (int i = 1; i < speed + 1; i++) {
			points.push_back(std::vector<point>());
			for (point pts : points[i - 1]) {
				for (int dir = 0; dir < 6; dir++) {
					point neighbour = hex_neighbor(pts, dir);
					if (std::find(visited.begin(), visited.end(), neighbour) == visited.end() &&
						!point_is_in_array(neighbour, obstacle, obstacle_size) &&
						!point_is_in_array(neighbour, vehicles_positions, vehicles_size)) {

						visited.push_back(neighbour);
						points[i].push_back(neighbour);
					}
				}
			}
		}

		delete[] vehicles_positions;
	}

	void ai::set_vehicles_positions(vehicles_native** vehicles, int* vehicles_size,
		point* obstacle, int obstacle_size) {

		/*
		0 - SPG
		1 - LT
		2 - HT
		3 - MT
		4 - ASPG
		*/

		int min_dist = 1000;
		int dist;
		const int base_size = DIRECTIONS_NUMBER + 1;
		point base[base_size];
		base[DIRECTIONS_NUMBER] = point{};

		//find MT and HT positions (in base)
		for (int dir = 0; dir < DIRECTIONS_NUMBER; dir++) {
			base[dir] = hex_neighbor(point{ 0, 0, 0 }, dir);
			dist = distance(base[dir], vehicles[2]->spawn_position);
			if (dist < min_dist) {
				min_dist = dist;
				positions[2] = base[dir];
			}
			else if (dist == min_dist) {
				positions[3] = base[dir];
			}
		}

		//find LT position (near base and enemy) and SPG position (near base and allies)
		point neighbour;
		min_dist = 0;
		for (int dir = 0; dir < DIRECTIONS_NUMBER; dir++) {
			neighbour = hex_neighbor(positions[2], dir);

			if (point_is_in_array(neighbour, base, base_size) ||
				point_is_in_array(neighbour, obstacle, obstacle_size))
				continue;

			dist = distance(neighbour, vehicles[3]->spawn_position);
			if (dist > min_dist) {
				min_dist = dist;
				positions[0] = positions[1];
				positions[1] = neighbour;
			}
			else
				positions[0] = neighbour;
		}

		//find ASPG position (far from base to shoot one side of base)
		for (int dir = 0; dir < DIRECTIONS_NUMBER; dir++) {
			neighbour = hex_neighbor(positions[0], dir);

			if (point_is_in_array(neighbour, base, base_size) ||
				point_is_in_array(neighbour, obstacle, obstacle_size))
				continue;

			if (positions[3].x == neighbour.x || positions[3].y == neighbour.y || positions[3].z == neighbour.z) {
				positions[4] = neighbour;
				break;
			}
		}
	}

	action ai::move_tank(vehicles_native* vehicles, int vehicles_size,
		vehicles_native** ally_vehicles, int* ally_vehicles_size,
		vehicles_native* vehicle,
		point* obstacle, int obstacle_size) {

		action return_action;
		return_action.action_type = action_type::move;
		return_action.vec_id = vehicle->vehicle_id;
		return_action.point = vehicle->position;

		int vehicle_goal_position = 0;
		point goal_position_LT = positions[1];
		switch (vehicle->vehicle_type) {
		case vehicle_type::SPG: {
			if (distance(ally_vehicles[2]->position, positions[2]) >=
				distance(vehicle->position, positions[2])) {
				return_action.action_type = action_type::nun;
				return return_action;
			}
		} break;
		case vehicle_type::LT: {
			vehicle_goal_position = 1;
			if (distance(ally_vehicles[2]->position, positions[2]) > 1)
				positions[1] = positions[2];
		} break;
		case vehicle_type::HT: {
			vehicle_goal_position = 2;
		} break;
		case vehicle_type::MT: {
			vehicle_goal_position = 3;
		} break;
		case vehicle_type::ASPG: {
			vehicle_goal_position = 4;
		} break;
		}

		if (distance(vehicle->position, positions[vehicle_goal_position]) == 0) {
			if (positions[1] == positions[2])
				positions[1] = goal_position_LT;
			return_action.action_type = action_type::nun;
			return return_action;
		}

		int speed = get_speed(vehicle->vehicle_type);
		std::vector<std::vector<point>> reachable_points;
		get_reachable_hexes(reachable_points, 
			obstacle, obstacle_size, 
			vehicles, vehicles_size,
			vehicle->position, speed);

		while (speed > 0) {
			if (reachable_points[speed].empty())
				speed--;
			else
				break;
		}

		if (speed == 0) {
			if (positions[1] == positions[2])
				positions[1] = goal_position_LT;
			return_action.action_type = action_type::nun;
			return return_action;
		}

		int dist;
		int min_dist = 1000;
		for (int i = 1; i < speed + 1; i++) {
			for (point pts : reachable_points[i]) {
				dist = distance(pts, positions[vehicle_goal_position]);
				if (dist <= min_dist) {
					min_dist = dist;
					return_action.point = pts;
				}
			}
		}

		if (positions[1] == positions[2])
			positions[1] = goal_position_LT;
		return return_action;
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