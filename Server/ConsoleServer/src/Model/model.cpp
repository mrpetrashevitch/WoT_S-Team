#include "model.h"

void to_json(nlohmann::json& j, const login& p)
{
	j = nlohmann::json{
		{"name", p.name},
		{"password", p.password},
		{"game", p.game},
		{"num_turns", p.num_turns},
		{"num_players", p.num_players},
		{"is_observer", p.is_observer},
	};
}

void from_json(const nlohmann::json& j, login& p)
{
	j.at("name").get_to(p.name);

	auto j_password = j.at("password");
	auto j_game = j.at("game");
	auto j_num_turns = j.at("num_turns");
	auto j_num_players = j.at("num_players");
	auto j_is_observer = j.at("is_observer");

	if (j_password.is_null()) p.password = ""; else j_password.get_to(p.password);
	if (j_game.is_null()) p.game = ""; else j_game.get_to(p.game);
	if (j_num_turns.is_null()) p.num_turns = 45; else j_num_turns.get_to(p.num_turns);
	if (j_num_players.is_null()) p.num_players = 1; else j_num_players.get_to(p.num_players);
	if (j_is_observer.is_null()) p.is_observer = false; else j_is_observer.get_to(p.is_observer);
}