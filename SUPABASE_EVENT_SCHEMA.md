# Supabase Database Schema for Events Feature

## Overview
This document outlines the required database tables and schemas for the Events feature in the CommunityFinder application.

## Required Tables

### 1. EventItem Table
Stores basic event information.

**Table Name**: `EventItem`

**Columns**:
- `EventId` (text, PRIMARY KEY) - Unique identifier for the event (from OnePa API)
- `Title` (text) - Event title
- `Outlet` (text) - Event outlet/location
- `StartDate` (timestamp with time zone, nullable) - Event start date
- `DateTimeText` (text, nullable) - Event date/time display text
- `MinPrice` (double precision, nullable) - Minimum price
- `MaxPrice` (double precision, nullable) - Maximum price
- `DetailUrl` (text, nullable) - URL to event detail page
- `Category` (text, nullable) - Event category/AOI
- `ViewCount` (integer, default 0) - Number of views
- `LikeCount` (integer, default 0) - Number of likes
- `FavoriteCount` (integer, default 0) - Number of favorites
- `RegisteredCount` (integer, default 0) - Number of registrations

**SQL Creation Script**:
```sql
CREATE TABLE "EventItem" (
    "EventId" text PRIMARY KEY,
    "Title" text NOT NULL,
    "Outlet" text,
    "StartDate" timestamp with time zone,
    "DateTimeText" text,
    "MinPrice" double precision,
    "MaxPrice" double precision,
    "DetailUrl" text,
    "Category" text,
    "ViewCount" integer DEFAULT 0,
    "LikeCount" integer DEFAULT 0,
    "FavoriteCount" integer DEFAULT 0,
    "RegisteredCount" integer DEFAULT 0
);
```

### 2. EventStatus Table
Tracks user interactions with events (likes, favorites, registrations).

**Table Name**: `EventStatus`

**Columns**:
- `id` (integer, PRIMARY KEY, auto-increment) - Unique record ID
- `user_id` (text, NOT NULL) - User UUID
- `event_id` (text, NOT NULL) - Event ID (foreign key to EventItem)
- `is_liked` (boolean, default false) - Whether user liked the event
- `is_favorited` (boolean, default false) - Whether user favorited the event
- `is_registered` (boolean, default false) - Whether user registered for the event
- `created_at` (timestamp with time zone, default now()) - Record creation time
- `updated_at` (timestamp with time zone, default now()) - Last update time

**Indexes**:
- Composite index on (user_id, event_id) for fast lookup
- Index on user_id for user-specific queries
- Index on event_id for event-specific queries

**SQL Creation Script**:
```sql
CREATE TABLE "EventStatus" (
    "id" serial PRIMARY KEY,
    "user_id" text NOT NULL,
    "event_id" text NOT NULL,
    "is_liked" boolean DEFAULT false,
    "is_favorited" boolean DEFAULT false,
    "is_registered" boolean DEFAULT false,
    "created_at" timestamp with time zone DEFAULT now(),
    "updated_at" timestamp with time zone DEFAULT now(),
    CONSTRAINT fk_event FOREIGN KEY ("event_id") REFERENCES "EventItem"("EventId") ON DELETE CASCADE,
    CONSTRAINT unique_user_event UNIQUE ("user_id", "event_id")
);

CREATE INDEX idx_event_status_user_id ON "EventStatus"("user_id");
CREATE INDEX idx_event_status_event_id ON "EventStatus"("event_id");
CREATE INDEX idx_event_status_user_event ON "EventStatus"("user_id", "event_id");
```

## Row Level Security (RLS)

### EventItem Table RLS Policies:
```sql
-- Enable RLS
ALTER TABLE "EventItem" ENABLE ROW LEVEL SECURITY;

-- Policy: Allow public read access
CREATE POLICY "Allow public read access on EventItem" 
ON "EventItem" FOR SELECT 
TO authenticated 
USING (true);

-- Policy: Allow authenticated users to insert
CREATE POLICY "Allow authenticated insert on EventItem" 
ON "EventItem" FOR INSERT 
TO authenticated 
WITH CHECK (true);

-- Policy: Allow authenticated users to update
CREATE POLICY "Allow authenticated update on EventItem" 
ON "EventItem" FOR UPDATE 
TO authenticated 
USING (true);
```

### EventStatus Table RLS Policies:
```sql
-- Enable RLS
ALTER TABLE "EventStatus" ENABLE ROW LEVEL SECURITY;

-- Policy: Users can only see their own status
CREATE POLICY "Users can view own event status" 
ON "EventStatus" FOR SELECT 
TO authenticated 
USING (auth.uid()::text = user_id);

-- Policy: Users can insert their own status
CREATE POLICY "Users can insert own event status" 
ON "EventStatus" FOR INSERT 
TO authenticated 
WITH CHECK (auth.uid()::text = user_id);

-- Policy: Users can update their own status
CREATE POLICY "Users can update own event status" 
ON "EventStatus" FOR UPDATE 
TO authenticated 
USING (auth.uid()::text = user_id);
```

## Setup Instructions

1. Log in to your Supabase dashboard
2. Navigate to the SQL Editor
3. Execute the SQL creation scripts above in order:
   - First create the EventItem table
   - Then create the EventStatus table with its indexes
4. Apply the RLS policies for both tables
5. Verify the tables are created correctly in the Table Editor

## Notes

- The `EventItem` table is similar to `CourseItem` but adapted for events
- The `EventStatus` table uses a different structure than `CourseStatus` (which uses arrays) for better query performance and flexibility
- Each user can only have one status record per event (enforced by unique constraint)
- Foreign key constraint ensures referential integrity between EventStatus and EventItem
- RLS policies ensure users can only manage their own event interactions
- The `is_registered` flag is set to true only once per user to prevent duplicate registration counts

## Integration with Existing Schema

The event feature integrates with the existing database by:
1. Using the same authentication system (auth.uid())
2. Following the same RLS pattern as other tables
3. Storing event data separately from course data for better organization
4. Using similar naming conventions for consistency
