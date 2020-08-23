using Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperAdventure
{
    public partial class SuperAdventure : Form

    {
        private Player _player;
        private Monster _currentMonster;

        public SuperAdventure()
        {
            InitializeComponent();

            Location location = new Location(1, "Home", "This is your house");

            _player = new Player(10, 10, 20, 0, 1);
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            _player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));




            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGoldPoints.Text = _player.Gold.ToString();
            lblExperiencePoints.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();
        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToTheNorth);
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToTheEast);

        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToTheSouth);
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToTheWest);
        }

        private void MoveTo(Location newLocation)
        {
            //Does the Location have any requires Items
            if (!_player.HasRequiredItemToEnterThisLocation(newLocation))
            {
                // we didnt find the required item in their inventory, so display a message and try to move
                rtbMessages.Text += "you must have a" + newLocation.ItemRequiredToEnter.Name + "to enter this Location"
                       + Environment.NewLine;
                return;

            }

            //Update the player's current Location
            _player.CurrentLocation = newLocation;

            //Show/Hide the available movement buttons
            btnNorth.Visible = (newLocation.LocationToTheNorth != null);
            btnEast.Visible = (newLocation.LocationToTheEast != null);
            btnSouth.Visible = (newLocation.LocationToTheSouth != null);
            btnWest.Visible = (newLocation.LocationToTheWest != null);

            // Display current location name and description
            rtbLocation.Text = newLocation.Name + Environment.NewLine;
            rtbLocation.Text += newLocation.Description + Environment.NewLine;

            //completely heal player
            _player.CurrentHitPoints = _player.MaximumHitPoints;

            //Update Hit points in UI
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();

            //Does the location have a quest?
            if (newLocation.QuestAvailableHere != null)
            {
                // see if the player has the quest, and if they've completed it
                bool playerAlreadyHasQuest = _player.HasThisQuest(newLocation.QuestAvailableHere);
                bool playerAlreadyCompletedQuest = _player.CompletedThisQuest(newLocation.QuestAvailableHere);



                // see if the player has the quest
                if (playerAlreadyHasQuest)
                {
                    //if the player has not completed the quest yet
                    if (!playerAlreadyCompletedQuest)
                    {

                        //see if the playeer has all the items needed to complete quest
                        bool playerHasAllItemsToCompleteQuest = _player.HasAllQuestCompletionItems(newLocation.QuestAvailableHere);

                        //The Player has all items required to complete the quest
                        if (playerHasAllItemsToCompleteQuest)
                        {
                            //Display message
                            rtbMessages.Text += Environment.NewLine;
                            rtbMessages.Text += "You complete the " + newLocation.QuestAvailableHere.Name + Environment.NewLine;

                            // Remove quest items from inventory
                            _player.RemoveQuestCompletionItems(newLocation.QuestAvailableHere);


                            //Give quest rewards
                            rtbMessages.Text += "you receive:" + Environment.NewLine;
                            rtbMessages.Text +=
                                newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() + "experience points" + Environment.NewLine;
                            rtbMessages.Text +=
                                newLocation.QuestAvailableHere.RewardGold.ToString() + "gold" + Environment.NewLine;
                            rtbMessages.Text +=
                                newLocation.QuestAvailableHere.RewardItem.Name + Environment.NewLine;
                            rtbMessages.Text += Environment.NewLine;

                            _player.ExperiencePoints +=
                                newLocation.QuestAvailableHere.RewardExperiencePoints;
                            _player.Gold += newLocation.QuestAvailableHere.RewardGold;

                            //Add the reward item to the player's inventory
                            _player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);

                            //Mark the quest as completed
                            _player.MarkQuestCompleted(newLocation.QuestAvailableHere);

                        }
                    }
                }

                else
                {
                    //The player does not already have the quest
                    //Display the messages
                    rtbMessages.Text += "You receive the " + newLocation.QuestAvailableHere.Name + "quest." + Environment.NewLine;
                    rtbMessages.Text += newLocation.QuestAvailableHere.Description + Environment.NewLine;
                    rtbMessages.Text += "To complete it, return with: " + Environment.NewLine;

                    foreach (QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        if (qci.Quantity == 1)
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + "" + qci.Details.Name + Environment.NewLine;

                        }
                        else
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + qci.Details.NamePlural + Environment.NewLine;
                        }

                        rtbMessages.Text += Environment.NewLine;

                        //Add the quest to the player's quest list
                        _player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
                    }
                }

                //Does the location have a monster?
                if (newLocation.MonsterLivingHere != null)
                {
                    rtbMessages.Text += "you see a " + newLocation.MonsterLivingHere.Name + Environment.NewLine;

                    //Make a new monster, using the values from the standard monster in the world.Monster list

                    Monster standardMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);

                    _currentMonster = new Monster(standardMonster.ID, standardMonster.Name, standardMonster.MaximumDamage,
                        standardMonster.RewardExperiencePoints, standardMonster.RewardGold, standardMonster.CurrentHitPoints,
                        standardMonster.MaximumHitPoints);

                    foreach (LootItem lootItem in standardMonster.LootTable)
                    {
                        _currentMonster.LootTable.Add(lootItem);
                    }

                    cboWeapons.Visible = true;
                    cboPotions.Visible = true;
                    btnUseWeapon.Visible = true;
                    btnUsePotion.Visible = true;
                }

                else
                {
                    _currentMonster = null;

                    cboWeapons.Visible = false;
                    cboPotions.Visible = false;
                    btnUseWeapon.Visible = false;
                    btnUsePotion.Visible = false;

                }

                //Update player's inventory list
                UpdateInventoryListInUI();

                //Refresh player's quest list
                UpdateQuestListInUI();

                //Refresh player's weapons combobox
                UpdateWeaponListInUI();

                // refresh player's  potions combobox
                UpdatePotionListInUI();


            }
            
        }

        private void UpdateInventoryListInUI()
        {
            dgvInventory.RowHeadersVisible = false;
            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Name";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Quantity";

            dgvInventory.Rows.Clear();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Quantity > 0)
                {
                    dgvInventory.Rows.Add(new[] { inventoryItem.Details.Name, inventoryItem.Quantity.ToString() });
                }
            }
        }

        private void UpdateQuestListInUI()
        {
            dgvQuests.RowHeadersVisible = false;

            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Name";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Done";

            dgvQuests.Rows.Clear();

            foreach (PlayerQuest playerQuest in _player.Quests)
            {
                dgvQuests.Rows.Add(new[] { playerQuest.Details.Name, playerQuest.IsCompleted.ToString() });
            }
        }

        private void UpdateWeaponListInUI()
        {
            List<Weapon> weapons = new List<Weapon>();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details is Weapon)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        weapons.Add((Weapon)inventoryItem.Details);
                    }
                }
            }

            if (weapons.Count == 0)
            {
                // The player doesn't have any weapons, so hide the weapon combobox and Use button
                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;

            }

            else
            {
                cboWeapons.DataSource = weapons;
                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";

                cboWeapons.SelectedIndex = 0;
            }
        }

        private void UpdatePotionListInUI()
        {
            List<HealingPotion> healingPotions = new List<HealingPotion>();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if (inventoryItem.Details is HealingPotion)
                {
                    if (inventoryItem.Quantity > 0)
                    {
                        healingPotions.Add((HealingPotion)inventoryItem.Details);
                    }
                }
            }

            if (healingPotions.Count == 0)
            {
                // The player doesn't have any potions, so hide the potion combobox and "Use" button
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;

            }

            else
            {
                cboPotions.DataSource = healingPotions;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";

                cboPotions.SelectedIndex = 0;


            }
        }


       






        public void btnUseWeapon_Click(object sender, EventArgs e)
        {
            //Get the currently selected weapon from the cmboweapons combobox
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

            //Determine the amount of dmage to do to the monster
            int damageToMonster = Engine.RandomNumberGenerator.NumberBetween(currentWeapon.MinimumDamage, currentWeapon.MaximumDamage);

            //Apply the damage to the monster's hitpoint
            _currentMonster.CurrentHitPoints -= damageToMonster;

            //Display Message
            rtbMessages.Text += "you hit the" + _currentMonster.Name + "for" + damageToMonster.ToString() + "points." + Environment.NewLine;

            // check if the monster is dead
            if (_currentMonster.CurrentHitPoints >= 0)
            {
                //Monster is dead
                rtbMessages.Text += Environment.NewLine;
                rtbMessages.Text += "you defeated the" + _currentMonster.Name + Environment.NewLine;



                //Give the player experience points for killing the monster
                _player.ExperiencePoints += _currentMonster.RewardExperiencePoints;
                rtbMessages.Text += "you receieve " + _currentMonster.RewardExperiencePoints.ToString() + "experience points" + Environment.NewLine;

                //Give player gold for killing the monster
                _player.Gold += _currentMonster.RewardGold;
                rtbMessages.Text += "you receive" + _currentMonster.RewardGold.ToString() + "gold." + Environment.NewLine;

                //Get rsndom loor from the monster
                List<InventoryItem> lootedItems = new List<InventoryItem>();

                //Add Items to the looted items list, comparing a random number to the drop percentage
                foreach (LootItem lootItem in _currentMonster.LootTable)
                {
                    if (Engine.RandomNumberGenerator.NumberBetween(1, 100) <= lootItem.DropPercentage)
                    {
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                    }
                }

                // If no Items were randomly selected, then add the default loot item(s) 
                if (lootedItems.Count == 0)
                {
                    foreach (LootItem lootItem in _currentMonster.LootTable)
                    {
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                    }
                }

                //Add the looted items to the player's inventory
                foreach (InventoryItem inventoryItem in lootedItems)
                {
                    _player.AddItemToInventory(inventoryItem.Details);

                    if (inventoryItem.Quantity == 1)
                    {
                        rtbMessages.Text += "you loot " + inventoryItem.Quantity.ToString() + "" + inventoryItem.Details.Name + Environment.NewLine;
                    }
                    else
                    {
                        rtbMessages.Text += "You loot " + inventoryItem.Quantity.ToString() + "" + inventoryItem.Details.NamePlural + Environment.NewLine;
                    }
                }

                //Refresh player information and inventory controls
                lblHitPoints.Text = _player.CurrentHitPoints.ToString();
                lblGold.Text = _player.Gold.ToString();
                lblExperiencePoints.Text = _player.ExperiencePoints.ToString();
                lblLevel.Text = _player.Level.ToString();

                UpdateInventoryListInUI();
                UpdateWeaponListInUI();
                UpdatePotionListInUI();

                //Add a blasnk line to the messages box, just for appearance.
                rtbMessages.Text += Environment.NewLine;

                //Move player to current location (to heal player and create a new monster to fight)
                MoveTo(_player.CurrentLocation);
            }

            else
            {
                //Monster is still alive

                //Determine the amount of damage the monster does to the player
                int damageToPlayer = Engine.RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

                //Display message
                rtbMessages.Text += "The " + _currentMonster.Name + "did" + damageToPlayer.ToString() + "Points of damage." + Environment.NewLine;

                //subtract damage from player
                _player.CurrentHitPoints -= damageToPlayer;

                //refresh player data in UI
                lblHitPoints.Text = _player.CurrentHitPoints.ToString();

                if(_player.CurrentHitPoints <= 0)
                {
                    //Display messages
                    rtbMessages.Text += "The " + _currentMonster.Name + "Killed you" + Environment.NewLine;

                    //move player to home
                    MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
                }
            }
        }


        

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            //Get the currently selected options for the combobox
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            //Add healing amount to the player's current hit points
            _player.CurrentHitPoints = (_player.CurrentHitPoints + potion.AmountToHeal);

            // CurrentHitPoints cannot exceed player's Maximum Hitpoints
            if(_player.CurrentHitPoints > _player.MaximumHitPoints)
            {
                _player.CurrentHitPoints = _player.MaximumHitPoints;
            }

            //Remove the potion from the player's list
            foreach(InventoryItem ii in _player.Inventory)
            {
                if (ii.Details.ID == potion.ID)
                {
                    ii.Quantity--;
                    break;
                }
            }

            //display message
            rtbMessages.Text += "You drink a " + potion.Name + Environment.NewLine;

            //Monster gets their turn to attack

            //Determine the ampunt of damage the monster does to the player
            int damageToPlayer = Engine.RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);

            //Display message
            rtbMessages.Text += "The " + _currentMonster.Name + "did" + damageToPlayer.ToString() + "points of damage." + Environment.NewLine;

            //Subtract damage from player
            _player.CurrentHitPoints -= damageToPlayer;

            if (_player.CurrentHitPoints <= 0)
            {
                rtbMessages.Text += "The " + _currentMonster.Name + "killed you" + Environment.NewLine;

                //Move player to home
                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            }

            //Refresh player data in UI
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            UpdateInventoryListInUI();
            UpdatePotionListInUI();

        }

        private void SuperAdventure_Load(object sender, EventArgs e)
        {

        }

    }

}


