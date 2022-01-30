#include "ai.h"
#include <cmath>
#include <algorithm>


namespace ai
{
	Result ai::get_action(int curr_player,
		player_native* players, int players_size,
		vehicles_native* vehicles, int vehicles_size,
		win_points_native* win_points, int win_points_size,
		AttackMatrix_native* attack_matrix, int attack_matrix_size,
		point* base, int base_size, action_ret* out_actions)
	{
			
			for (int j = 0, i = 0; j < vehicles_size; j++)
			{
				/*if (vehicles[j].player_id == curr_player)
				{
					out_actions->actions[i].action_type = action_type::move;
					out_actions->actions[i].vec_id = vehicles[j].vehicle_id;
					out_actions->actions[i].point = vehicles[j].position;
					out_actions->actions[i].point.x++;
					out_actions->actions[i].point.z--;
					i++;
				}*/
				if (vehicles[j].player_id != curr_player) {
					continue;
				}
				vehicles_native** targets = new vehicles_native*[vehicles_size];
				int targets_size = 0;
				get_targets(curr_player, vehicles, vehicles_size, vehicles[j].position, targets, &targets_size);
				bool is_shoot_ok = false;
				for (int k = 0; k < targets_size; k++) {
					if (check_neutrality(curr_player, targets[k]->player_id, attack_matrix, attack_matrix_size)) {
						is_shoot_ok = true;
						out_actions->actions[i].action_type = action_type::shoot;
						out_actions->actions[i].vec_id = vehicles[j].vehicle_id;
						out_actions->actions[i].point = targets[k] -> position;
						i++;
						break;
					}
				}
				if (is_shoot_ok) {
					continue;
				}
				out_actions->actions[i].action_type = action_type::move;
				out_actions->actions[i].vec_id = vehicles[j].vehicle_id;
				out_actions->actions[i].point = vehicles[j].position;
				/*if (base->x < vehicles[j].position.x) {
					out_actions->actions[i].point.x -= 2;
					out_actions->actions[i].point.y++;
					out_actions->actions[i].point.z++;
				}
				else if (base->y < vehicles[j].position.y) {
					out_actions->actions[i].point.x++;
					out_actions->actions[i].point.y -= 2;
					out_actions->actions[i].point.z++;
				}
				else if (base->z < vehicles[j].position.z) {
					out_actions->actions[i].point.x++;
					out_actions->actions[i].point.y++;
					out_actions->actions[i].point.z -= 2;
				}*/
				point base_center = point();
				int min_value = distance(vehicles[j].position, base_center);
				for (int dx = -2; dx <= 2; dx++) {
					for (int dy = -2; dy <= 2; dy++) {
						int dz = 0 - dx - dy;
						point new_point = vehicles[j].position;
						new_point.x += dx;
						new_point.y += dy;
						new_point.z += dz;
						int dist0 = distance(vehicles[j].position, new_point);
						if (dist0 > 2 || 
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
							out_actions->actions[i].point = new_point;
							min_value = dist_to_base;
						}
					}
				}
				vehicles[j].position = out_actions->actions[i].point;
				i++;
			}
		return Result::OKEY;
	}

	void ai::get_targets(int curr_player,
		vehicles_native* vehicles, int vehicles_size,
		point position,
		vehicles_native** targets, int* targets_size)
	{

		int i = 0;
		for (int j = 0; j < vehicles_size; j++) {
			if (vehicles[j].player_id != curr_player && distance(position, vehicles[j].position) == 2
				&& vehicles[j].health != 0) {
				targets[i] = &vehicles[j];
				i++;
			}
		}
		std::sort(targets, targets + i, [](vehicles_native* l, vehicles_native* r) {return l->health < r->health; });
		*targets_size = i;

	}

	int ai::distance(point a, point b)
	{
		return (abs(a.x - b.x) + abs(a.y - b.y) + abs(a.z - b.z)) / 2;
	}

	bool ai::check_neutrality(int curr_player, int goal,
		AttackMatrix_native* attack_matrix, int attack_matrix_size)
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
