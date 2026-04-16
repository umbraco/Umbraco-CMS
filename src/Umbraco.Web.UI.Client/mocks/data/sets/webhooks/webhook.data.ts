import type { UmbMockWebhookModel } from '../../mock-data-set.types.js';

// Content type IDs referenced from the default mock data set
const SIMPLE_DOC_TYPE_ID = 'the-simplest-document-type-id';
const SIMPLE_DOC_TYPE_2_ID = 'simple-document-type-id';

// ─── Reusable event definitions ──────────────────────────────────────────────

const EVENT_CONTENT_PUBLISHED = { eventName: 'Content Published', eventType: 'Content', alias: 'Umbraco.ContentPublish' };
const EVENT_CONTENT_SAVED = { eventName: 'Content Saved', eventType: 'Content', alias: 'Umbraco.ContentSaved' };
const EVENT_CONTENT_DELETED = { eventName: 'Content Deleted', eventType: 'Content', alias: 'Umbraco.ContentDelete' };
const EVENT_CONTENT_UNPUBLISHED = { eventName: 'Content Unpublished', eventType: 'Content', alias: 'Umbraco.ContentUnpublish' };
const EVENT_MEDIA_SAVED = { eventName: 'Media Saved', eventType: 'Media', alias: 'Umbraco.MediaSave' };
const EVENT_MEDIA_DELETED = { eventName: 'Media Deleted', eventType: 'Media', alias: 'Umbraco.MediaDelete' };
const EVENT_MEDIA_RECYCLED = { eventName: 'Media Moved to Recycle Bin', eventType: 'Media', alias: 'mediaMovedToRecycleBin' };
const EVENT_MEMBER_SAVED = { eventName: 'Member Saved', eventType: 'Member', alias: 'memberSaved' };
const EVENT_MEMBER_DELETED = { eventName: 'Member Deleted', eventType: 'Member', alias: 'memberDeleted' };
const EVENT_MEMBER_ROLES_ASSIGNED = { eventName: 'Member Roles Assigned', eventType: 'Member', alias: 'assignedMemberRoles' };
const EVENT_USER_LOGIN_SUCCESS = { eventName: 'User Login Success', eventType: 'Other', alias: 'userLoginSuccess' };
const EVENT_USER_LOGIN_FAILED = { eventName: 'User Login Failed', eventType: 'Other', alias: 'userLoginFailed' };
const EVENT_USER_LOCKED = { eventName: 'User Locked', eventType: 'Other', alias: 'userLocked' };

// ─── Webhook mock data ────────────────────────────────────────────────────────

export const data: Array<UmbMockWebhookModel> = [
	// 1 — Minimal: no name, no description, no headers, no content type filter
	{
		id: 'webhook-minimal-id',
		name: null,
		description: null,
		url: 'https://example.com/webhook',
		enabled: true,
		events: [EVENT_CONTENT_PUBLISHED],
		contentTypeKeys: [],
		headers: {},
	},

	// 2 — Named with description
	{
		id: 'webhook-named-id',
		name: 'Content Publisher',
		description: 'Fires whenever content is published to the live site.',
		url: 'https://example.com/on-publish',
		enabled: true,
		events: [EVENT_CONTENT_PUBLISHED],
		contentTypeKeys: [],
		headers: {},
	},

	// 3 — Disabled webhook
	{
		id: 'webhook-disabled-id',
		name: 'Disabled Integration',
		description: null,
		url: 'https://example.com/disabled',
		enabled: false,
		events: [EVENT_CONTENT_SAVED],
		contentTypeKeys: [],
		headers: {},
	},

	// 4 — Single content type filter
	{
		id: 'webhook-single-content-type-id',
		name: 'Blog Post Publisher',
		description: 'Only fires when a specific document type is published.',
		url: 'https://blog.example.com/on-publish',
		enabled: true,
		events: [EVENT_CONTENT_PUBLISHED],
		contentTypeKeys: [SIMPLE_DOC_TYPE_ID],
		headers: {},
	},

	// 5 — Multiple content type filters
	{
		id: 'webhook-multi-content-type-id',
		name: 'Multi-type Watcher',
		description: null,
		url: 'https://example.com/multi-type',
		enabled: true,
		events: [EVENT_CONTENT_PUBLISHED, EVENT_CONTENT_UNPUBLISHED],
		contentTypeKeys: [SIMPLE_DOC_TYPE_ID, SIMPLE_DOC_TYPE_2_ID],
		headers: {},
	},

	// 6 — Single auth header
	{
		id: 'webhook-single-header-id',
		name: 'Authenticated Webhook',
		description: 'Sends a Bearer token with every request.',
		url: 'https://api.example.com/webhook',
		enabled: true,
		events: [EVENT_CONTENT_PUBLISHED],
		contentTypeKeys: [],
		headers: { Authorization: 'Bearer super-secret-token' },
	},

	// 7 — Multiple headers
	{
		id: 'webhook-multi-header-id',
		name: 'Multi-Header Endpoint',
		description: null,
		url: 'https://api.example.com/media-webhook',
		enabled: true,
		events: [EVENT_MEDIA_SAVED],
		contentTypeKeys: [],
		headers: {
			'X-API-Key': 'abc123',
			'X-Environment': 'production',
			'X-Client-Id': 'umbraco-cms',
		},
	},

	// 8 — Multiple events, single category (Content lifecycle)
	{
		id: 'webhook-content-lifecycle-id',
		name: 'Content Lifecycle',
		description: 'Tracks the full lifecycle of a content item from save through delete.',
		url: 'https://example.com/content-lifecycle',
		enabled: true,
		events: [EVENT_CONTENT_SAVED, EVENT_CONTENT_PUBLISHED, EVENT_CONTENT_UNPUBLISHED, EVENT_CONTENT_DELETED],
		contentTypeKeys: [],
		headers: {},
	},

	// 9 — Multiple events across categories (Content + Media + Member)
	{
		id: 'webhook-cross-category-id',
		name: 'Cross-Category Notifier',
		description: null,
		url: 'https://example.com/all-types',
		enabled: true,
		events: [EVENT_CONTENT_PUBLISHED, EVENT_MEDIA_SAVED, EVENT_MEMBER_SAVED],
		contentTypeKeys: [],
		headers: {},
	},

	// 10 — Media events
	{
		id: 'webhook-media-id',
		name: 'Media Watcher',
		description: 'Monitors all media create, update, and delete operations.',
		url: 'https://media.example.com/webhook',
		enabled: true,
		events: [EVENT_MEDIA_SAVED, EVENT_MEDIA_DELETED, EVENT_MEDIA_RECYCLED],
		contentTypeKeys: [],
		headers: {},
	},

	// 11 — Member events
	{
		id: 'webhook-member-id',
		name: 'Member Activity',
		description: null,
		url: 'https://members.example.com/webhook',
		enabled: true,
		events: [EVENT_MEMBER_SAVED, EVENT_MEMBER_DELETED, EVENT_MEMBER_ROLES_ASSIGNED],
		contentTypeKeys: [],
		headers: {},
	},

	// 12 — Other/User events
	{
		id: 'webhook-user-events-id',
		name: 'User Activity Monitor',
		description: 'Security audit webhook that fires on login, failure, and account lock events.',
		url: 'https://security.example.com/webhook',
		enabled: true,
		events: [EVENT_USER_LOGIN_SUCCESS, EVENT_USER_LOGIN_FAILED, EVENT_USER_LOCKED],
		contentTypeKeys: [],
		headers: {},
	},

	// 13 — Content type filter + auth header (combined)
	{
		id: 'webhook-filtered-auth-id',
		name: 'Secure Filtered Webhook',
		description: 'Filtered to a specific content type with Bearer token authentication.',
		url: 'https://secure.example.com/webhook',
		enabled: true,
		events: [EVENT_CONTENT_PUBLISHED, EVENT_CONTENT_UNPUBLISHED],
		contentTypeKeys: [SIMPLE_DOC_TYPE_ID],
		headers: { Authorization: 'Bearer prod-token-xyz', 'X-Environment': 'production' },
	},

	// 14 — Disabled with full configuration
	{
		id: 'webhook-disabled-full-id',
		name: 'Legacy Integration',
		description: 'Previously active integration that has been disabled. Kept for reference.',
		url: 'https://old-system.example.com/webhook/v1',
		enabled: false,
		events: [EVENT_CONTENT_PUBLISHED, EVENT_CONTENT_DELETED],
		contentTypeKeys: [SIMPLE_DOC_TYPE_2_ID],
		headers: { 'X-API-Key': 'old-key-abc', Authorization: 'Bearer deprecated-token' },
	},

	// 15 — Long values (edge case)
	{
		id: 'webhook-long-values-id',
		name: 'Very Long Webhook Name That Tests The Maximum Display Width In The List And Detail Views',
		description:
			'This is an intentionally long description to verify that the UI handles multi-line or truncated text gracefully. It covers the scenario where an editor adds detailed documentation about what this webhook does, why it exists, and which external system it connects to.',
		url: 'https://example.com/webhooks/endpoint/with/a/very/deep/nested/path?env=production&version=2&client=umbraco',
		enabled: true,
		events: [EVENT_CONTENT_PUBLISHED],
		contentTypeKeys: [],
		headers: {},
	},
];
