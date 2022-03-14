#pragma once
#include "../Model/model.h"

namespace engine
{
	struct vehicle_param
	{
		int hp_max = 0;
		int speed = 0;
		int damage = 0;
		int gold = 0;
		int shoot_min = 0;
		int shoot_max = 0;
	};

	class battle
	{
		vehicle_param _vehicle_params[5]{
			{2,2,1,2,2,2},
			{1,3,1,1,2,2},
			{3,1,1,3,1,2},
			{2,1,1,2,1,3},
			{1,1,1,1,3,3}
		};
		std::string _name{};
		models::game_state _game_state;
		models::map _map;
		models::action_rsp _actions;

		void _init_map();
		void _init_game_state();

		models::vehicle _create_vehicle(models::vehicle_type type, int player_index, const models::point3& pos);
		void _add_vehicles(int player_index);

		int _vehicle_id = 1;

	public:
		battle(std::string name, int num_players, int num_turns);
		bool add_player(const models::player& player);

		models::player get_player_by_name(const std::string name);
		const models::map& get_map();
		const models::game_state& get_game_state();
		const models::action_rsp& get_actions();
	};
}
