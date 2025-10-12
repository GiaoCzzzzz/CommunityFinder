# AI Course Assistant Service Test Examples

## Test Cases

### Test Case 1: Chinese - Weekend Sports Course
**Input:** "我想找周末的运动课程"

**Expected Output:**
- Day: "Saturday"
- SuggestedCategory: "Sports & Fitness"
- SearchText: ""
- AIResponse: Contains "Saturday" and "Sports & Fitness"

### Test Case 2: English - Morning Music Class
**Input:** "Show me music classes in the morning"

**Expected Output:**
- Time: "Morning"
- SuggestedCategory: "Music & Dance"
- SearchText: "music classes"
- AIResponse: Contains "Morning" and "Music & Dance"

### Test Case 3: Mixed - Programming Course Wednesday
**Input:** "Find programming courses on Wednesday"

**Expected Output:**
- Day: "Wednesday"
- SuggestedCategory: "Technology"
- SearchText: "programming courses"
- AIResponse: Contains "Wednesday" and "Technology"

### Test Case 4: Chinese - Art Class Evening
**Input:** "周三晚上有什么艺术课？"

**Expected Output:**
- Day: "Wednesday"
- Time: "Evening"
- SuggestedCategory: "Arts & Crafts"
- SearchText: ""
- AIResponse: Contains "Wednesday", "Evening", and "Arts & Crafts"

### Test Case 5: English - Fitness Friday
**Input:** "I want fitness classes on Friday"

**Expected Output:**
- Day: "Friday"
- SuggestedCategory: "Sports & Fitness"
- SearchText: "fitness classes"
- AIResponse: Contains "Friday" and "Sports & Fitness"

### Test Case 6: Dance Afternoon
**Input:** "dancing in the afternoon"

**Expected Output:**
- Time: "Afternoon"
- SuggestedCategory: "Music & Dance"
- SearchText: "dancing"
- AIResponse: Contains "Afternoon" and "Music & Dance"

### Test Case 7: Technology Saturday Morning
**Input:** "coding classes on Saturday morning"

**Expected Output:**
- Day: "Saturday"
- Time: "Morning"
- SuggestedCategory: "Technology"
- SearchText: "coding classes"
- AIResponse: Contains "Saturday", "Morning", and "Technology"

### Test Case 8: Chinese - Education Sunday
**Input:** "星期日的教育课程"

**Expected Output:**
- Day: "Sunday"
- SuggestedCategory: "Education & Enrichment"
- SearchText: ""
- AIResponse: Contains "Sunday" and "Education & Enrichment"

### Test Case 9: Complex Query
**Input:** "I'm looking for art and craft workshops on Monday evening near Jurong"

**Expected Output:**
- Day: "Monday"
- Time: "Evening"
- SuggestedCategory: "Arts & Crafts"
- SearchText: "art craft workshops jurong"
- AIResponse: Contains "Monday", "Evening", "Arts & Crafts"

### Test Case 10: Minimal Query
**Input:** "sports"

**Expected Output:**
- SuggestedCategory: "Sports & Fitness"
- SearchText: "sports"
- AIResponse: Contains "Sports & Fitness"

## Manual Testing Instructions

1. Open the application and navigate to MainPage
2. Click the 🤖 AI Assistant button in the top right
3. Enter each test case input in the chat
4. Verify the AI response matches expected output
5. Click "Apply Filters and Return" button
6. Verify the filters are applied correctly on MainPage
7. Click the search button to see filtered results

## Integration Testing

To test the full integration:

1. Start with a fresh MainPage load
2. Open AI Assistant
3. Enter: "我想找周六早上的运动课"
4. Verify AI detects: Day=Saturday, Time=Morning, Category=Sports
5. Click Apply
6. Verify MainPage shows:
   - SelectedDay = "Saturday"
   - SelectedTime = "Morning"
7. Click Search button
8. Verify course list shows only Saturday morning sports courses

## Edge Cases

### Empty Input
**Input:** ""
**Expected:** No action, prompt user to enter text

### Unrecognized Input
**Input:** "asdfghjkl"
**Expected:** AI response suggests examples

### Very Long Input
**Input:** "I want to find some really good courses that teach programming and coding and software development and web design for beginners on weekends either Saturday or Sunday preferably in the morning but afternoon is also okay"
**Expected:** Extracts: Day=Saturday, Time=Morning, Category=Technology, SearchText contains relevant keywords

### Special Characters
**Input:** "programming!!! @#$%"
**Expected:** Filters special chars, extracts "programming"

## Performance Testing

- Response time should be < 100ms for typical queries
- UI should update smoothly without lag
- Chat history should scroll automatically to latest message

## Accessibility Testing

- Test with screen readers
- Verify keyboard navigation works
- Check color contrast for messages
- Ensure touch targets are large enough (min 44x44 dp)
