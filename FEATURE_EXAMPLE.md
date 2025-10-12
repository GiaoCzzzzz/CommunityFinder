# AI Interest Analysis Feature - Example Usage

## Feature Description
When a user enters the MainPage, the system automatically:
1. Fetches the user's interests from their Supabase profile
2. Analyzes these interests against available course categories
3. Auto-fills the course filter dropdowns (What/Category/Topic)
4. Displays relevant courses immediately

## Example Scenarios

### Scenario 1: User interested in Music
**User Profile:**
```json
{
  "interest": ["guitar", "music", "instruments"]
}
```

**Result:**
- **What (L1):** Lifestyle & Leisure
- **Category (L2):** Music & Vocal Courses
- **Topic (L3):** Guitar

**Courses Displayed:** Guitar lessons, music theory, instrument classes

---

### Scenario 2: User interested in Programming
**User Profile:**
```json
{
  "interest": ["coding", "programming", "technology", "software development"]
}
```

**Result:**
- **What (L1):** Lifelong Learning
- **Category (L2):** Digital, Tech & Innovation Courses
- **Topic (L3):** Software Application

**Courses Displayed:** Programming courses, software development, app development

---

### Scenario 3: User interested in Fitness
**User Profile:**
```json
{
  "interest": ["yoga", "fitness", "health", "wellness"]
}
```

**Result:**
- **What (L1):** Health & Wellness
- **Category (L2):** Personal Health & Nutrition Courses or Fitness-related
- **Topic (L3):** Related health/fitness topics

**Courses Displayed:** Yoga classes, fitness programs, wellness workshops

---

### Scenario 4: User interested in Cooking
**User Profile:**
```json
{
  "interest": ["cooking", "baking", "culinary arts", "food"]
}
```

**Result:**
- **What (L1):** Lifestyle & Leisure
- **Category (L2):** Culinary Courses
- **Topic (L3):** Chinese Cooking, Western Cuisine, or Baking

**Courses Displayed:** Cooking classes, baking workshops, culinary courses

---

### Scenario 5: User interested in Language Learning
**User Profile:**
```json
{
  "interest": ["learn chinese", "language", "mandarin"]
}
```

**Result:**
- **What (L1):** Education & Enrichment
- **Category (L2):** Primary Tuition Courses or Preschool Courses
- **Topic (L3):** Chinese Language

**Courses Displayed:** Chinese language courses, Mandarin classes

---

## How the Matching Algorithm Works

### 1. Keyword Extraction
The system extracts meaningful keywords from user interests:
- "guitar music lessons" → ["guitar", "music", "lessons"]
- Removes common stop words: "a", "an", "the", "and", etc.

### 2. Direct Matching
Checks if interest keywords appear in category names:
- **High Score (10 pts):** Match in L3 (most specific) - e.g., "guitar" matches "Guitar" topic
- **Medium Score (5 pts):** Match in L2 (category) - e.g., "music" matches "Music & Vocal Courses"
- **Low Score (2 pts):** Match in L1 (top level) - e.g., "learning" matches "Lifelong Learning"

### 3. Semantic Matching
Additional scoring for related concepts:
- **Education keywords** → "Education & Enrichment" category
- **Health keywords** → "Health & Wellness" category
- **Art/Craft keywords** → Arts & Handicrafts categories
- **Music keywords** → "Music & Vocal Courses"
- **Dance keywords** → "Dance Courses"
- **Sports keywords** → Sports & Fitness categories
- **Tech keywords** → "Digital, Tech & Innovation Courses"

### 4. Best Match Selection
The system:
1. Calculates scores for all possible L1/L2/L3 combinations
2. Selects the combination with the highest score
3. Only auto-fills if score exceeds threshold (0.1)
4. Falls back to manual selection if no good match found

---

## User Experience Flow

```
User Opens MainPage
        ↓
[Loading Categories...]
        ↓
[Analyzing Interests...]
        ↓
✓ Auto-filled: Music & Vocal Courses > Guitar
        ↓
[Loading Relevant Courses...]
        ↓
Display: 15 Guitar courses found
```

---

## Technical Implementation

### Files Modified:
1. **Services/InterestAnalyzer.cs** (NEW)
   - Core matching algorithm
   - Keyword extraction and semantic analysis

2. **MainPage.xaml.cs**
   - Added `AutoFillCategoriesFromInterests()` method
   - Integrated into `OnAppearing()` lifecycle

3. **ViewModels/CoursesViewModel.cs**
   - Added `SetCategorySelection()` method
   - Allows programmatic category selection

4. **Services/AuthService.cs**
   - Fixed `GetInterest()` to fetch current user's interests

### Database Schema:
```sql
-- profiles table
{
  id: uuid (primary key),
  username: string,
  interest: text[] (array of interest strings),
  ...
}
```

---

## Benefits

✅ **Personalized Experience** - Immediate relevant results
✅ **Time Saving** - No manual category navigation
✅ **Discovery** - Find courses user didn't know existed
✅ **Non-intrusive** - Can always override selections
✅ **Fail-safe** - Errors don't break the app

---

## Future Enhancements

- [ ] Machine learning model for better accuracy
- [ ] Multiple category recommendations
- [ ] User feedback on recommendations
- [ ] A/B testing different matching algorithms
- [ ] Performance caching
- [ ] Recommendation explanations ("Recommended because you like X")
