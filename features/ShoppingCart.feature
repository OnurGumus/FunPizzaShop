Feature: Shpping Cart

Scenario: Add to Cart 
Given I configured a pizza
When I click order in confiugation
Then pizza should be added to cart

Scenario: Remove from Cart
Given There is an item in the cart
When I click remove
Then the item should be removed from the cart

Scenario: Review Order
Given there are items in the cart
When I click order in sidebar
Then review order screen should be visible
