# Forum Feature - Supabase Schema Documentation

## Overview
This document describes the database schema required for the Forum feature in the CommunityFinder app.

## Tables Required

### 1. forum_categories
Stores the main forum categories.

```sql
CREATE TABLE forum_categories (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL,
    display_order INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Default categories
INSERT INTO forum_categories (name, display_order) VALUES
    ('Training Provider Directory', 1),
    ('Education & Enrichment', 2),
    ('Health & Wellness', 3),
    ('Lifelong Learning', 4),
    ('Lifestyle & Leisure', 5),
    ('Sports & Fitness', 6);
```

### 2. forum_posts
Stores all forum posts.

```sql
CREATE TABLE forum_posts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    category_id UUID NOT NULL REFERENCES forum_categories(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
    username TEXT NOT NULL,
    topic TEXT NOT NULL,
    content TEXT NOT NULL,
    post_type TEXT NOT NULL CHECK (post_type IN ('Ask a Question', 'Sharing posts')),
    linked_course_id TEXT,
    linked_event_id TEXT,
    reply_count INTEGER DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Index for faster queries
CREATE INDEX idx_forum_posts_category ON forum_posts(category_id);
CREATE INDEX idx_forum_posts_user ON forum_posts(user_id);
CREATE INDEX idx_forum_posts_created ON forum_posts(created_at DESC);
```

### 3. forum_replies
Stores all replies to posts.

```sql
CREATE TABLE forum_replies (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    post_id UUID NOT NULL REFERENCES forum_posts(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
    username TEXT NOT NULL,
    content TEXT NOT NULL,
    parent_reply_id UUID REFERENCES forum_replies(id) ON DELETE CASCADE,
    linked_course_id TEXT,
    linked_event_id TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Index for faster queries
CREATE INDEX idx_forum_replies_post ON forum_replies(post_id);
CREATE INDEX idx_forum_replies_parent ON forum_replies(parent_reply_id);
CREATE INDEX idx_forum_replies_created ON forum_replies(created_at ASC);
```

### 4. forum_reports
Stores user reports for admin review.

```sql
CREATE TABLE forum_reports (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    reporter_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
    post_id UUID REFERENCES forum_posts(id) ON DELETE CASCADE,
    reply_id UUID REFERENCES forum_replies(id) ON DELETE CASCADE,
    reason TEXT DEFAULT 'Inappropriate content',
    status TEXT NOT NULL DEFAULT 'pending' CHECK (status IN ('pending', 'resolved', 'dismissed')),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    CONSTRAINT check_report_target CHECK (
        (post_id IS NOT NULL AND reply_id IS NULL) OR
        (post_id IS NULL AND reply_id IS NOT NULL)
    )
);

-- Index for faster queries
CREATE INDEX idx_forum_reports_status ON forum_reports(status);
CREATE INDEX idx_forum_reports_created ON forum_reports(created_at DESC);
```

### 5. forum_announcements
Stores announcements for each category.

```sql
CREATE TABLE forum_announcements (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    category_id UUID NOT NULL REFERENCES forum_categories(id) ON DELETE CASCADE,
    content TEXT NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(category_id)
);
```

## Row Level Security (RLS) Policies

### forum_categories
```sql
-- Enable RLS
ALTER TABLE forum_categories ENABLE ROW LEVEL SECURITY;

-- Everyone can read categories
CREATE POLICY "Categories are viewable by everyone"
    ON forum_categories FOR SELECT
    USING (true);

-- Only admin can insert/update/delete
CREATE POLICY "Only admin can manage categories"
    ON forum_categories FOR ALL
    USING (auth.jwt() ->> 'email' = 'chen2004peter@gmail.com');
```

### forum_posts
```sql
-- Enable RLS
ALTER TABLE forum_posts ENABLE ROW LEVEL SECURITY;

-- Everyone can read posts
CREATE POLICY "Posts are viewable by everyone"
    ON forum_posts FOR SELECT
    USING (true);

-- Authenticated users can create posts
CREATE POLICY "Authenticated users can create posts"
    ON forum_posts FOR INSERT
    WITH CHECK (auth.uid() = user_id);

-- Users can update their own posts
CREATE POLICY "Users can update own posts"
    ON forum_posts FOR UPDATE
    USING (auth.uid() = user_id);

-- Users can delete their own posts, or admin can delete any
CREATE POLICY "Users can delete own posts or admin can delete any"
    ON forum_posts FOR DELETE
    USING (auth.uid() = user_id OR auth.jwt() ->> 'email' = 'chen2004peter@gmail.com');
```

### forum_replies
```sql
-- Enable RLS
ALTER TABLE forum_replies ENABLE ROW LEVEL SECURITY;

-- Everyone can read replies
CREATE POLICY "Replies are viewable by everyone"
    ON forum_replies FOR SELECT
    USING (true);

-- Authenticated users can create replies
CREATE POLICY "Authenticated users can create replies"
    ON forum_replies FOR INSERT
    WITH CHECK (auth.uid() = user_id);

-- Users can delete their own replies, or admin can delete any
CREATE POLICY "Users can delete own replies or admin can delete any"
    ON forum_replies FOR DELETE
    USING (auth.uid() = user_id OR auth.jwt() ->> 'email' = 'chen2004peter@gmail.com');
```

### forum_reports
```sql
-- Enable RLS
ALTER TABLE forum_reports ENABLE ROW LEVEL SECURITY;

-- Users can create reports
CREATE POLICY "Authenticated users can create reports"
    ON forum_reports FOR INSERT
    WITH CHECK (auth.uid() = reporter_id);

-- Only admin can view reports
CREATE POLICY "Only admin can view reports"
    ON forum_reports FOR SELECT
    USING (auth.jwt() ->> 'email' = 'chen2004peter@gmail.com');

-- Only admin can update reports
CREATE POLICY "Only admin can update reports"
    ON forum_reports FOR UPDATE
    USING (auth.jwt() ->> 'email' = 'chen2004peter@gmail.com');
```

### forum_announcements
```sql
-- Enable RLS
ALTER TABLE forum_announcements ENABLE ROW LEVEL SECURITY;

-- Everyone can read announcements
CREATE POLICY "Announcements are viewable by everyone"
    ON forum_announcements FOR SELECT
    USING (true);

-- Only admin can manage announcements
CREATE POLICY "Only admin can manage announcements"
    ON forum_announcements FOR ALL
    USING (auth.jwt() ->> 'email' = 'chen2004peter@gmail.com');
```

## Admin Account
The admin account is set to: **chen2004peter@gmail.com**

All admin-specific operations (managing categories, announcements, viewing reports, and enhanced deletion capabilities) are restricted to this email address.

## Features Implemented

### User Features
1. **Forum Categories Page**: Browse 6 default categories in a 2-column grid
2. **Search**: Search posts by topic title
3. **Category Detail Page**: View posts in a category with announcement, filter by post type
4. **Create Post**: Create posts with topic, content, type, and optional course/event links
5. **Post Detail Page**: View posts and replies in Baidu Tieba style structure
6. **Reply System**: Reply to posts and replies (nested structure)
7. **My Posts Page**: View own posts and replies received
8. **Report System**: Report posts and replies to admin
9. **Course/Event Linking**: Link courses and events from History page to posts and replies

### Admin Features
1. **Category Management**: Add new categories
2. **Announcement Management**: Edit announcements for each category
3. **Reports & Alerts**: View all reported content
4. **Enhanced Deletion**: Delete any post or reply with all sub-content
5. **Report Resolution**: Mark reports as resolved

## Integration Notes
- The Forum service integrates with existing AuthService for authentication
- Course and Event linking uses existing CourseItem and EventItem tables
- History page is reused for selecting courses/events to link
- Admin status is checked by email address: chen2004peter@gmail.com
