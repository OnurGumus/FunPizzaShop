Feature: Shpping Cart

Scenario: Add to Cart 
Given I configured a pizza
When I order a configuration
Then pizza should be added to cart

Scenario: Remove from Cart
Given There is an item in the cart
When I remove that item
Then the item should be removed from the cart

Scenario: Review Order
Given there are items in the cart
When I order from  sidebar
Then order should be reviewed
