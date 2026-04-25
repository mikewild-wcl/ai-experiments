name: recipe-writer
description: Create a recipe using the ingredients the user already has, with an optional requested dish or cuisine. Example: "Help me cook something nice with {{ ingredients }}" or "Give me a recipe for {{ dish }} using {{ ingredients }}."
license: Apache-2.0
compatibility: Requires python3
metadata:
  author: mike
  version: "2.1"
---

# Recipe Writer

Generate a practical recipe based on the user's available ingredients.

## Inputs

- Available ingredients: `{{ ingredients }}`
- Optional requested dish: `{{ dish }}`

## Instructions

When this skill is invoked:

1. Use the ingredients in `{{ ingredients }}` as the primary source for the recipe.
2. If `{{ dish }}` is provided, try to match that dish as closely as possible.
3. Prefer simple, realistic recipes that can be cooked at home.
4. Avoid adding many extra ingredients the user did not mention.
5. If a small number of common pantry items are needed, list them separately as optional extras.
6. If the requested dish is not feasible with the provided ingredients, suggest the closest alternative.
7. If there are not enough ingredients for a full recipe, suggest 2 to 3 meal ideas instead.

## Output Format

Return the result in this structure:

### Recipe Name

**Servings:**  
**Prep Time:**  
**Cook Time:**

### Ingredients
- List the ingredients the user has
- List any optional pantry extras separately

### Instructions
1. Step one
2. Step two
3. Step three

### Notes
- Substitutions
- Tips
- Storage or serving suggestions

## Style

- Keep the tone helpful and concise.
- Prioritize clear cooking steps.
- Make the recipe easy to follow 