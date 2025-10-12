# Interest Analyzer Feature

## Overview
The Interest Analyzer automatically matches user interests with course categories to provide a personalized experience when users enter the MainPage.

## How It Works

### 1. Data Flow
```
User enters MainPage
    ↓
Fetch user interests from Supabase (profiles.interest array)
    ↓
Load categories from categories.txt
    ↓
Analyze interests using InterestAnalyzer
    ↓
Find best matching L1/L2/L3 category
    ↓
Auto-fill course filter dropdowns (What/Category/Topic)
    ↓
Search courses with selected filters
```

### 2. InterestAnalyzer Service
Located in `Services/InterestAnalyzer.cs`

#### Key Methods:
- **LoadCategories(string categoryText)**: Parses the categories.txt file to build the category tree
- **AnalyzeInterests(string[] interests)**: Matches user interests against categories and returns the best L1/L2/L3 match
- **CalculateMatchScore()**: Uses keyword matching and semantic analysis to score category matches

#### Matching Algorithm:
1. **Keyword Extraction**: Normalizes text and extracts meaningful keywords
2. **Direct Matching**: Checks if interest keywords appear in category names
3. **Weighted Scoring**:
   - L3 (most specific) matches: 10 points
   - L2 (category) matches: 5 points
   - L1 (top level) matches: 2 points
   - Exact matches: +20 points bonus
4. **Semantic Matching**: Additional scoring for related concepts:
   - Education/learning → Education & Enrichment
   - Health/fitness/yoga → Health & Wellness
   - Art/craft/creative → Arts & Crafts
   - Music/instruments → Music & Vocal Courses
   - Dance → Dance Courses
   - Sports → Sports & Fitness
   - Language → Language courses
   - Tech/coding/digital → Digital, Tech & Innovation
   - Cooking/baking → Culinary Courses

### 3. Integration Points

#### MainPage.xaml.cs
- **OnAppearing()**: Triggers the auto-fill process when page appears
- **AutoFillCategoriesFromInterests()**: Orchestrates the interest analysis
- **LoadCategoriesText()**: Loads categories.txt from app resources

#### CoursesViewModel.cs
- **SetCategorySelection(string l1, string l2, string l3)**: Programmatically sets the filter selections
- **InitAsync()**: Loads and parses categories.txt
- **SearchByAoiAsync()**: Searches courses based on selected filters

#### AuthService.cs
- **GetInterest()**: Fetches the current user's interests from Supabase profiles table

## Example Usage

### User Interest: ["music", "guitar"]
Results in:
- L1: "Lifestyle & Leisure"
- L2: "Music & Vocal Courses"
- L3: "Guitar"

### User Interest: ["cooking", "baking"]
Results in:
- L1: "Lifestyle & Leisure"
- L2: "Culinary Courses" or "Baking Courses"
- L3: Appropriate subcategory

### User Interest: ["programming", "technology"]
Results in:
- L1: "Lifelong Learning"
- L2: "Digital, Tech & Innovation Courses"
- L3: "Software Application" or "Mobile App Application"

## Error Handling
- If user has no interests: Silently continues without auto-fill
- If no match found: User can manually select categories
- If Supabase request fails: Error is caught and user experience is not affected
- If categories.txt cannot be loaded: Falls back to default behavior

## Benefits
1. **Personalized Experience**: Users see relevant courses immediately
2. **Time Saving**: No need to manually navigate category dropdowns
3. **Discovery**: Users may discover courses related to their interests they didn't know existed
4. **Seamless Integration**: Works transparently in the background
5. **Non-intrusive**: Failures don't break the app, user can always override selections

## Future Enhancements
- Add machine learning model for better matching
- Support multiple category recommendations
- Track which auto-selections lead to course enrollments
- Add user feedback on recommendation quality
- Cache analysis results to improve performance
