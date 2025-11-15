# Forum Feature Implementation Guide

## Overview
The Forum feature is a comprehensive discussion platform similar to Reddit or Baidu Tieba, allowing users to share and discuss courses and activities. This feature is fully integrated with Supabase for data persistence.

## Architecture

### Models
All forum models are located in `CommunityFinder/Models/`:

1. **ForumCategory** - Stores forum categories
2. **ForumPost** - Stores user posts with topic, content, and metadata
3. **ForumReply** - Stores replies to posts (supports nested replies)
4. **ForumReport** - Stores user reports for admin review
5. **ForumAnnouncement** - Stores announcements for each category

### Services
**ForumService** (`CommunityFinder/Services/ForumService.cs`):
- Handles all Forum CRUD operations
- Manages admin operations (categories, announcements, reports)
- Integrates with existing CourseItem and EventItem tables
- Implements admin permission checks

### Views
All forum pages are in `CommunityFinder/Views/`:

1. **ForumCategoriesPage** - Main forum page with category grid
2. **ForumCategoryDetailPage** - Shows posts in a category with filtering
3. **CreatePostPage** - Create new posts with course/event linking
4. **PostDetailPage** - Shows post and replies in Baidu Tieba style
5. **MyPostsPage** - Shows user's posts and received replies
6. **AdminAlertsPage** - Admin-only page for viewing reports
7. **ForumSearchResultsPage** - Displays search results

## Features

### User Features

#### 1. Forum Categories
- **Layout**: 2-column grid layout
- **Default Categories**:
  1. Training Provider Directory
  2. Education & Enrichment
  3. Health & Wellness
  4. Lifelong Learning
  5. Lifestyle & Leisure
  6. Sports & Fitness
- **Navigation**: Tap any category to view its posts
- **Search**: Search bar at the top searches all posts by topic

#### 2. Category Detail Page
- **Announcement Section**: Shows category-specific announcements
- **Post Type Filter**: Filter by "Ask a Question" or "Sharing posts"
- **Post Cards Display**:
  - Username (left top)
  - Topic (center, bold)
  - Reply count (below topic)
  - Post type badge (right top)
  - Report button (right bottom)

#### 3. Creating Posts
- **Fields**:
  - Topic (required)
  - Content (required)
  - Post Type (required): "Ask a Question" or "Sharing posts"
  - Optional: Link to a course or event from history
- **Course/Event Linking**: 
  - Click "+" button to open History page in selection mode
  - Select a course or event to link to the post
  - Linked items appear as clickable cards in the post

#### 4. Post Detail Page (Baidu Tieba Style)
- **Main Post (Floor 0)**:
  - Author username with (OP) badge
  - Post date
  - Topic (title)
  - Content
  - Linked course/event card (if any)
  
- **Replies (Floors)**:
  - Each reply is a separate card
  - Shows username, content, date
  - Reply button (for main replies only)
  - Report button
  - Admin delete button (admins only)
  
- **Nested Replies**:
  - Sub-replies are visually indented
  - Shows "Username → ParentUsername" format
  - Cannot be replied to (only 2-level depth)

- **Reply Input**:
  - Text input at bottom
  - "+" button to add course/event link
  - "Send" button to submit

#### 5. My Posts Page
Two tabs:
- **My Posts**: 
  - Shows all user's posts
  - Cards show category, topic, reply count, date
  - Delete button to remove post (with confirmation)
  
- **Replies to Your Posts**:
  - Shows all replies others made to your posts
  - Cards show username, content
  - Report button
  - Tap to navigate to the post

#### 6. Search
- Search by post topic from main forum page
- Results show all matching posts across categories
- Tap any result to view the post detail

#### 7. Reporting
- Users can report posts and replies
- Reports are sent to admin for review
- Report reason: "Inappropriate content" (default)

### Admin Features
**Admin Account**: chen2004peter@gmail.com

#### 1. Category Management
- Create new categories via "Add Category" button on main page
- Categories are displayed in order by display_order

#### 2. Announcement Management
- Edit announcements for each category
- Announcements appear at the top of category detail pages
- Can be plain text or formatted content

#### 3. Reports & Alerts Page
- View all pending reports
- Reports show:
  - Report type (Post or Reply)
  - Report date
  - Reason
- Actions:
  - View: Navigate to the reported content
  - Mark Resolved: Close the report

#### 4. Enhanced Deletion
- Admins can delete any post or reply
- Deleting a post removes all its replies
- Deleting a reply removes all its sub-replies
- Delete buttons appear on post detail pages for admins

## Integration Points

### With History Page
The HistoryPage now supports a "selection mode":
- When opened from CreatePostPage or PostDetailPage, it enters selection mode
- User selects a course or event from their history or favorites
- Selection is passed back to the calling page via ItemSelected event
- Selected item appears as a linked card in the post/reply

### With Course/Event Pages
- Linked courses/events appear as clickable cards
- Tapping a linked item navigates to CourseDetailPage or EventDetailPage
- Uses existing navigation infrastructure

### With Auth System
- Uses existing AuthService for authentication
- Admin check based on email: chen2004peter@gmail.com
- User ID from CurrentSession.User.Id
- Username from Profiles table

## Database Schema

See `FORUM_SCHEMA.md` for complete database schema including:
- Table definitions
- Row Level Security (RLS) policies
- Indexes for performance
- Admin permissions

### Key Tables
1. `forum_categories` - Category definitions
2. `forum_posts` - All posts
3. `forum_replies` - All replies (with parent_reply_id for nesting)
4. `forum_reports` - User reports
5. `forum_announcements` - Category announcements

## Setup Instructions

### 1. Supabase Setup
Execute the SQL from `FORUM_SCHEMA.md` in your Supabase project:
1. Create all tables
2. Set up RLS policies
3. Insert default categories
4. Create indexes

### 2. Code Integration
The code is already integrated:
- ForumService registered in MauiProgram.cs
- Navigation added to MainPage (Forum tab)
- All XAML pages registered in .csproj

### 3. Testing
1. Login with a regular user account
2. Navigate to Forum from main page
3. Browse categories
4. Create a post
5. Reply to posts
6. Test reporting functionality

Admin testing:
1. Login with chen2004peter@gmail.com
2. Verify admin buttons appear
3. Test category creation
4. Test announcement editing
5. Test viewing reports
6. Test admin deletion capabilities

## Navigation Flow

```
MainPage (Forum Tab)
  → ForumCategoriesPage
     → Search → ForumSearchResultsPage → PostDetailPage
     → Category → ForumCategoryDetailPage
        → Create Post → CreatePostPage
           → Add Link → HistoryPage (selection mode)
        → Post → PostDetailPage
           → Reply with Link → HistoryPage (selection mode)
           → Linked Item → CourseDetailPage/EventDetailPage
     → My Posts → MyPostsPage
        → Post → PostDetailPage
     → Admin Only: AdminAlertsPage (from settings or special navigation)
```

## Code Style

The implementation follows the existing project patterns:
- MVVM pattern where appropriate
- ObservableCollections for dynamic lists
- Async/await for all database operations
- Try-catch with user-friendly error messages
- Consistent naming conventions
- Proper null checking

## Performance Considerations

1. **Lazy Loading**: Posts are loaded per category
2. **Filtering**: Post type filtering done in memory after load
3. **Indexes**: Database indexes on foreign keys and frequently queried fields
4. **Caching**: Category list cached on initial load
5. **RLS**: Database-level security reduces API attack surface

## Future Enhancements (Not Implemented)

1. **Post Editing**: Allow users to edit their own posts
2. **Like System**: Add likes/votes for posts and replies
3. **Notifications**: Notify users when their posts receive replies
4. **Rich Text**: Add markdown or rich text support
5. **Images**: Allow image uploads in posts
6. **Pagination**: Paginate replies for very long threads
7. **Categories**: Allow users to follow specific categories
8. **Search Enhancement**: Full-text search on content, not just topics
9. **Moderation Tools**: More admin tools like user bans, content moderation
10. **Analytics**: Track popular posts, active users, etc.

## Known Limitations

1. **History Selection**: Currently requires users to have items in history/favorites to link
2. **Two-Level Replies**: Reply nesting limited to 2 levels (main reply → sub-reply)
3. **No Edit**: Posts and replies cannot be edited after creation
4. **Single Admin**: Admin status hardcoded to one email address
5. **No Media**: No image or file upload support
6. **Basic Search**: Search only matches topic titles, not content

## Troubleshooting

### Issue: "Failed to load categories"
- Check Supabase connection
- Verify forum_categories table exists
- Check RLS policies allow SELECT

### Issue: "Failed to create post"
- Verify user is authenticated
- Check forum_posts table exists
- Verify RLS policies allow INSERT for authenticated users

### Issue: Admin features not showing
- Confirm logged in as chen2004peter@gmail.com
- Check email in CurrentSession.User.Email

### Issue: Links not working
- Verify CourseItem/EventItem exists in database
- Check if item was deleted from source

## Support

For issues or questions, please refer to:
1. This README
2. FORUM_SCHEMA.md for database schema
3. Code comments in ForumService.cs
4. Existing documentation for Course/Event features
