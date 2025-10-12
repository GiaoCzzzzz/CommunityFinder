# AI Interest Analysis Feature - Technical Flow

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         User Opens MainPage                      │
└─────────────────────────────┬───────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    OnAppearing() - MainPage                      │
│  1. await _vm.InitAsync()                                        │
│  2. await AutoFillCategoriesFromInterests()                      │
│  3. await _vm.SearchByAoiAsync(maxPages: 8)                     │
└─────────────────────────────┬───────────────────────────────────┘
                              │
              ┌───────────────┴───────────────┐
              │                               │
              ▼                               ▼
┌─────────────────────────┐     ┌─────────────────────────┐
│   InitAsync() [Step 1]  │     │ AutoFillCategories      │
│   - Load categories.txt │     │   [Step 2]              │
│   - Parse category tree │     │                         │
│   - Populate L1Options  │     │   1. GetInterest()      │
└─────────────────────────┘     │   2. LoadCategories()   │
                                │   3. AnalyzeInterests() │
                                │   4. SetCategorySelect()│
                                └───────────┬─────────────┘
                                            │
                                            ▼
                          ┌─────────────────────────────────┐
                          │    AuthService.GetInterest()    │
                          │  - Get current user GUID        │
                          │  - Query Supabase profiles      │
                          │  - Return interest[] array      │
                          └──────────────┬──────────────────┘
                                         │
                                         ▼
                          ┌─────────────────────────────────┐
                          │   InterestAnalyzer              │
                          │  .LoadCategories(text)          │
                          │  - Parse L1/L2/L3 structure     │
                          │  - Build category tree          │
                          └──────────────┬──────────────────┘
                                         │
                                         ▼
                          ┌─────────────────────────────────┐
                          │   InterestAnalyzer              │
                          │  .AnalyzeInterests(interests)   │
                          │  - Extract keywords             │
                          │  - Match against categories     │
                          │  - Calculate scores             │
                          │  - Return best match (L1,L2,L3) │
                          └──────────────┬──────────────────┘
                                         │
                                         ▼
                          ┌─────────────────────────────────┐
                          │   CoursesViewModel              │
                          │  .SetCategorySelection()        │
                          │  - Verify L1 exists in tree     │
                          │  - Set SelectedL1 = L1          │
                          │  - Verify L2 exists under L1    │
                          │  - Set SelectedL2 = L2          │
                          │  - Verify L3 exists under L2    │
                          │  - Set SelectedL3 = L3          │
                          └──────────────┬──────────────────┘
                                         │
                                         ▼
                          ┌─────────────────────────────────┐
                          │  Property Change Events Fired   │
                          │  - SelectedL1 → RefillL2()      │
                          │  - SelectedL2 → RefillL3()      │
                          │  - UI dropdowns auto-update     │
                          └──────────────┬──────────────────┘
                                         │
                                         ▼
                          ┌─────────────────────────────────┐
                          │   SearchByAoiAsync() [Step 3]   │
                          │  - Build URL with L1/L2/L3      │
                          │  - Fetch courses from OnePA     │
                          │  - Apply additional filters     │
                          │  - Display results to user      │
                          └─────────────────────────────────┘
```

## Data Flow Details

### Step 1: Fetch User Interests
```csharp
// AuthService.GetInterest()
Input:  Current user session
Query:  SELECT interest FROM profiles WHERE id = user_guid
Output: ["guitar", "music", "jazz"]
```

### Step 2: Load & Parse Categories
```csharp
// InterestAnalyzer.LoadCategories()
Input:  categories.txt content
Parse:  一级Education & Enrichment
        二级Enrichment Courses
        三级：
        Abacus & Mental
        Lifeskills
        ...
Output: Dictionary<L1, Dictionary<L2, List<L3>>>
```

### Step 3: Analyze Interests
```csharp
// InterestAnalyzer.AnalyzeInterests()
Input:  interests = ["guitar", "music", "jazz"]

Process:
  For each category path (L1/L2/L3):
    - Extract keywords from category names
    - Match against interest keywords
    - Calculate weighted score:
      * L3 match = 10 points
      * L2 match = 5 points  
      * L1 match = 2 points
    - Add semantic bonuses
  
  Select highest scoring path

Output: (L1: "Lifestyle & Leisure", 
         L2: "Music & Vocal Courses", 
         L3: "Guitar")
```

### Step 4: Set Category Selection
```csharp
// CoursesViewModel.SetCategorySelection(L1, L2, L3)
Input:  L1="Lifestyle & Leisure"
        L2="Music & Vocal Courses"
        L3="Guitar"

Process:
  1. Verify _aoiTree contains L1
  2. Set SelectedL1 = "Lifestyle & Leisure"
     → Triggers RefillL2() via property change
  3. Verify _aoiTree[L1] contains L2
  4. Set SelectedL2 = "Music & Vocal Courses"
     → Triggers RefillL3() via property change
  5. Verify _aoiTree[L1][L2] contains L3
  6. Set SelectedL3 = "Guitar"

Result: UI dropdowns now show selected values
```

### Step 5: Search Courses
```csharp
// CoursesViewModel.SearchByAoiAsync()
Input:  SelectedL1 = "Lifestyle & Leisure"
        SelectedL2 = "Music & Vocal Courses"
        SelectedL3 = "Guitar"

Process:
  1. Slug the category names:
     - L2: "music-vocal" (drop "Courses", slug)
     - L3: "guitar" (slug)
  
  2. Build URL:
     https://www.onepa.gov.sg/pacesapi/coursessearch/searchjson
     ?aoilname=Guitar
     &aoil2=music-vocal
     &aoil3=guitar
     &page=1
  
  3. Fetch courses from API
  4. Apply local filters (Where, Day, Time, SearchText)
  5. Display in UI

Output: List of guitar courses displayed to user
```

## Scoring Algorithm Example

### Input: interests = ["guitar", "music"]

### Candidates Evaluated:

#### Option A: Music & Vocal Courses → Guitar
```
Keywords matched:
- "guitar" in L3 "Guitar" = 10 pts (exact match) + 20 pts (bonus)
- "music" in L2 "Music & Vocal Courses" = 5 pts
- Semantic bonus: music-related = 3 pts
Total Score: 38 pts ✓ BEST MATCH
```

#### Option B: Handicrafts → Instrument Craft
```
Keywords matched:
- "guitar" might match "instrument" = 2 pts (loose match)
- No other matches
Total Score: 2 pts
```

#### Option C: Education & Enrichment → Music & Movement
```
Keywords matched:
- "music" in L3 "Music & Movement" = 10 pts
- Semantic bonus: education-related = 3 pts
Total Score: 13 pts
```

**Winner: Option A (Score: 38)** → Selected automatically

## Error Handling

```
Try to auto-fill categories
  ├─ User not logged in?
  │   └─ Skip silently, continue with defaults
  │
  ├─ User has no interests?
  │   └─ Skip silently, continue with defaults
  │
  ├─ Supabase query fails?
  │   └─ Catch exception, log error, continue with defaults
  │
  ├─ Categories.txt not found?
  │   └─ Skip silently, use fallback
  │
  ├─ No category match found?
  │   └─ Skip silently, user selects manually
  │
  └─ Category match found?
      └─ Set selections and trigger search ✓
```

## Performance Characteristics

- **Category parsing**: O(n) where n = lines in categories.txt
- **Interest analysis**: O(m × c) where m = interests, c = categories
- **Typical execution time**: < 100ms
- **Memory usage**: Minimal (small category tree)
- **Network calls**: 1 (fetch interests from Supabase)

## Testing Scenarios

1. ✓ User with interests → Categories auto-filled
2. ✓ User with no interests → Default behavior
3. ✓ User not logged in → Skip auto-fill
4. ✓ Network error → Graceful degradation
5. ✓ Invalid interest data → Skip auto-fill
6. ✓ No category match → User selects manually
7. ✓ Multiple good matches → Select highest score
