﻿using Spellfire.Dal;
using Spellfire.Model;
using Spellfire.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Spellfire.Web.Controllers
{
    public class CardController : BaseController
    {
        private const int MaxCardListCount = 10;
        private IDataAccess _dal;

        public CardController(IDataAccess dal)
        {
            _dal = dal;
        }

        public ActionResult Index(string searchText)
        {
            var viewModel = new HomeViewModel()
            {
                SearchText = searchText ?? "spellfire",
            };

            return View(viewModel);
        }

        public ActionResult List(string searchText, bool includeOnlineBoosters)
        {
            var cards = _dal.Cards.GetByName(searchText, includeOnlineBoosters, x => x.CardKinds, x => x.Booster);
            var filteredCards = cards.Take(MaxCardListCount);

            foreach (var card in filteredCards)
            {
                _dal.Cards.LoadCollection(card, c => c.CardKinds, null, c => c.Kind);
            }

            var viewModel = new HomeViewModel()
            {
                SearchText = searchText,
                SearchCount = cards.Count(),
                FilteredCards = filteredCards
            };

            return PartialView("_CardList", viewModel);
        }

        public ActionResult Details(int id = 0)
        {
            var card = _dal.Cards.GetBySequenceNumber(id, c => c.Booster, c => c.Rarity, c => c.World);

            _dal.Cards.LoadCollection(card, c => c.CardCharacteristics, null, c => c.Characteristic);
            _dal.Cards.LoadCollection(card, c => c.CardKinds, null, c => c.Kind);
            _dal.Cards.LoadCollection(card, c => c.CardPhases);

            var viewModel = new HomeViewModel()
            {
                SelectedCard = card
            };

            return PartialView("_CardDetail", viewModel);
        }

        public ActionResult Hand()
        {
            // Rule     - ADD:2196, ADD2:none, BR:3140, DS:2258, DR:none, DL:2370,  FR:2552, GH:1694, NS:none, RL:2259
            // Dungeon  - ADD:3444, ADD2:none, BR:3445, DS:3446, DR:none, DL:3447,  FR:3448, GH:3449, NS:none, RL:3450
            // Other    - ADD:none, ADD2:none, BR:none, DS:none, DR:none, DL:none,  FR:none, GH:none, NS:none, RL:none
            //var sequenceNumbers = new List<int>() 
            //                      { 
            //                        2196, 3140, 2258, 2370, 2552, 1694, 2259,   // Rule
            //                        3444, 3445, 3446, 3447, 3448, 3449, 3450    // Dungeon
            //                      };

            // oneCharLevel - Cleric:1992, Hero:2325, Monster:3300, Psionicist:2723, Realm:3473, Regent:3160, Thief:3361, Wizard:3222
            // twoCharLevel - Ally:2677, Artifact:2440, BloodAbility:3103, ClericSpell:2102, Holding*:1508, MagItem:3096, PsionicPower:2791, ThiefSkill:3533, UnarmedCombat:3038, WizardSpell:1371
            //                Cleric:3488, Hero:2181, Monster:2194, Psionicist:3564, Realm:3478, Realm:1773, Regent:3159, Thief:3363, Wizard:3439
            //
            // * TODO: fix the database for holdings without "+"
            var sequenceNumbers = new List<int>() 
                                  { 
                                    2677, 2440, 3103, 2102, 1508, 3096, 2791, 3478, 1773, 3159, 3363, 3038, 1371,
                                    3488, 2181, 2194, 3564, 3439
                                  };

            var viewModel = new HomeViewModel()
            {
                FilteredCards = GetFullCards(sequenceNumbers)
            };

            return View("Hand", viewModel);
        }

        private List<Card> GetFullCards(List<int> sequenceNumbers)
        {
            var cards = new List<Card>();

            foreach (var sequenceNumber in sequenceNumbers)
            {
                var card = GetFullCard(sequenceNumber);

                if (card != null)
                {
                    cards.Add(card);
                }
            }

            return cards;
        }

        private Card GetFullCard(int sequenceNumber)
        {
            var card = _dal.Cards.GetBySequenceNumber(sequenceNumber, c => c.Booster, c => c.Rarity, c => c.World);

            if (card == null)
            {
                return null;
            }

            _dal.Cards.LoadCollection(card, c => c.CardKinds, null, c => c.Kind);

            return card;
        }
    }
}