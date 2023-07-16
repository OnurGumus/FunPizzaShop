Feature: Configure Pizza

Scenario: Show Menu
When I visit main page
Then pizza menu should be visible

Scenario: Configure Pizza
Given I am in the main page
When I click a pizza
Then configure pizza screen should be visible
