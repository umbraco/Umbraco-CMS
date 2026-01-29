/* eslint-disable @typescript-eslint/naming-convention */
/* eslint-disable local-rules/enforce-umbraco-external-imports */
/**
 * Database connection and helper functions for SQLite to mock data transformation.
 */
import path from 'path';
import fs from 'fs';
import { fileURLToPath } from 'url';
import initSqlJs from 'sql.js';
import type { BindParams, Database as SqlJsDatabase } from 'sql.js';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Configuration - must be set before using database
let _dbPath: string | null = null;
let _setAlias: string | null = null;
let _db: SqlJsDatabase | null = null;

/**
 * Configure the database module with paths.
 * Must be called before using any database functions.
 */
export function configure(dbPath: string, setAlias: string): void {
	_dbPath = dbPath;
	_setAlias = setAlias;
}

/**
 * Get the output directory for generated mock data files.
 * Throws if configure() has not been called.
 */
export function getOutputDir(): string {
	if (!_setAlias) {
		throw new Error('db.ts not configured. Call configure() first.');
	}
	return path.resolve(__dirname, `../../src/mocks/data/sets/${_setAlias}`);
}

/**
 * Get the database instance, initializing it if needed.
 * Throws if configure() has not been called.
 */
export async function getDatabase(): Promise<SqlJsDatabase> {
	if (!_dbPath) {
		throw new Error('db.ts not configured. Call configure() first.');
	}
	if (!_db) {
		const SQL = await initSqlJs();
		const dbBuffer = fs.readFileSync(_dbPath);
		_db = new SQL.Database(dbBuffer);
	}
	return _db;
}

/**
 * Close the database connection.
 */
export function closeDatabase(): void {
	if (_db) {
		_db.close();
		_db = null;
	}
}

/**
 * Get the configured database path.
 */
export function getDbPath(): string | null {
	return _dbPath;
}

/**
 * Helper class to create a consistent query interface similar to better-sqlite3
 */
export class PreparedStatement {
	constructor(
		private database: SqlJsDatabase,
		private sql: string,
	) {}

	all(...params: unknown[]): unknown[] {
		const stmt = this.database.prepare(this.sql);
		const results: unknown[] = [];

		// Bind parameters if provided
		if (params.length > 0) {
			stmt.bind(params as BindParams);
		}

		while (stmt.step()) {
			const row = stmt.getAsObject();
			results.push(row);
		}

		stmt.free();
		return results;
	}

	get(...params: unknown[]): unknown {
		const results = this.all(...params);
		return results[0] || null;
	}
}

/**
 * Prepare a SQL statement.
 * Requires that getDatabase() has been called first to initialize the database.
 */
export function prepare(sql: string): PreparedStatement {
	if (!_db) {
		throw new Error('Database not initialized. Call getDatabase() first.');
	}
	return new PreparedStatement(_db, sql);
}

/**
 * Object type GUIDs in Umbraco
 */
export const ObjectTypes = {
	DataType: '30A2A501-1978-4DDB-A57B-F7EFED43BA3C',
	DocumentType: 'A2CB7800-F571-4787-9638-BC48539A0EFB',
	MediaType: '4EA4382B-2F5A-4C2B-9587-AE9B3CF3602E',
	MemberType: '9B5416FB-E72F-45A9-A07B-5A9A2709CE43',
	Document: 'C66BA18E-EAF3-4CFF-8A22-41B16D66A972',
	Media: 'B796F64C-1F99-4FFB-B886-4BF4BC011A9C',
	Member: '39EB0F98-B348-42A1-8662-E7EB18487560',
	Template: '6FBDE604-4178-42CE-A10B-8A2600A2F07D',
	DataTypeContainer: '521231E3-8B37-469C-9F9D-51AFC91FEB7B',
	DocumentTypeContainer: '2F7A2769-6B0B-4468-90DD-AF42D64F7F16',
	MediaTypeContainer: '42D26947-8A7C-4F25-BD14-60B0BF5BD0B4',
	RecycleBin: '01BB7FF2-24DC-4C0C-95A2-C24EF72BBAC8', // Content recycle bin
	MediaRecycleBin: 'CF3D8E34-1C1C-41E9-AE56-878B57B32113', // Media recycle bin
} as const;

/**
 * Common interfaces for database rows
 */
export interface UmbracoNode {
	id: number;
	uniqueId: string;
	parentId: number;
	level: number;
	path: string;
	sortOrder: number;
	trashed: number;
	nodeUser: number | null;
	text: string | null;
	nodeObjectType: string | null;
	createDate: string;
}

export interface DataType {
	nodeId: number;
	propertyEditorAlias: string;
	dbType: string;
	config: string | null;
}

export interface ContentType {
	nodeId: number;
	alias: string;
	icon: string | null;
	thumbnail: string | null;
	description: string | null;
	listView: string | null;
	allowAtRoot: number;
	variations: number;
	isElement: number;
}

export interface PropertyType {
	id: number;
	dataTypeId: number;
	contentTypeId: number;
	propertyTypeGroupId: number | null;
	alias: string;
	name: string | null;
	sortOrder: number;
	mandatory: number;
	mandatoryMessage: string | null;
	validationRegExp: string | null;
	validationRegExpMessage: string | null;
	description: string | null;
	variations: number;
	labelOnTop: number;
}

export interface PropertyTypeGroup {
	id: number;
	uniqueID: string;
	contenttypeNodeId: number;
	type: number;
	text: string | null;
	alias: string | null;
	sortorder: number;
}

export interface User {
	id: number;
	userDisabled: number;
	userNoConsole: number;
	userName: string;
	userLogin: string;
	userPassword: string;
	passwordConfig: string | null;
	userEmail: string;
	userLanguage: string | null;
	createDate: string;
	updateDate: string;
	avatar: string | null;
	failedLoginAttempts: number | null;
	lastLockoutDate: string | null;
	lastPasswordChangeDate: string | null;
	lastLoginDate: string | null;
	emailConfirmedDate: string | null;
	invitedDate: string | null;
	securityStampToken: string | null;
	key: string;
}

export interface UserGroup {
	id: number;
	userGroupAlias: string;
	userGroupName: string;
	userGroupDefaultPermissions: string | null;
	createDate: string;
	updateDate: string;
	icon: string | null;
	hasAccessToAllLanguages: number;
	startContentId: number | null;
	startMediaId: number | null;
	key: string;
}

export interface Language {
	id: number;
	languageISOCode: string;
	languageCultureName: string;
	isDefaultVariantLang: number;
	mandatory: number;
	fallbackLanguageId: number | null;
}

export interface Template {
	nodeId: number;
	alias: string;
	// Note: design/content is stored on disk, not in database
}

/**
 * Map property editor aliases to editor UI aliases
 */
export function getEditorUiAlias(propertyEditorAlias: string): string {
	const mapping: Record<string, string> = {
		// Text editors
		'Umbraco.TextBox': 'Umb.PropertyEditorUi.TextBox',
		'Umbraco.TextArea': 'Umb.PropertyEditorUi.TextArea',
		'Umbraco.Plain.String': 'Umb.PropertyEditorUi.TextBox',
		'Umbraco.Plain.Integer': 'Umb.PropertyEditorUi.Integer',
		'Umbraco.Plain.Decimal': 'Umb.PropertyEditorUi.Decimal',

		// Rich text
		'Umbraco.RichText': 'Umb.PropertyEditorUi.Tiptap',
		'Umbraco.MarkdownEditor': 'Umb.PropertyEditorUi.MarkdownEditor',

		// Pickers
		'Umbraco.ContentPicker': 'Umb.PropertyEditorUi.DocumentPicker',
		'Umbraco.MediaPicker3': 'Umb.PropertyEditorUi.MediaPicker',
		'Umbraco.MultiNodeTreePicker': 'Umb.PropertyEditorUi.ContentPicker',
		'Umbraco.MemberPicker': 'Umb.PropertyEditorUi.MemberPicker',
		'Umbraco.MemberGroupPicker': 'Umb.PropertyEditorUi.MemberGroupPicker',
		'Umbraco.UserPicker': 'Umb.PropertyEditorUi.UserPicker',
		'Umbraco.MultiUrlPicker': 'Umb.PropertyEditorUi.MultiUrlPicker',
		'Umbraco.ImageCropper': 'Umb.PropertyEditorUi.ImageCropper',
		'Umbraco.UploadField': 'Umb.PropertyEditorUi.UploadField',

		// Lists and choices
		'Umbraco.DropDown.Flexible': 'Umb.PropertyEditorUi.Dropdown',
		'Umbraco.CheckBoxList': 'Umb.PropertyEditorUi.CheckBoxList',
		'Umbraco.RadioButtonList': 'Umb.PropertyEditorUi.RadioButtonList',
		'Umbraco.TrueFalse': 'Umb.PropertyEditorUi.Toggle',

		// Numbers
		'Umbraco.Integer': 'Umb.PropertyEditorUi.Integer',
		'Umbraco.Decimal': 'Umb.PropertyEditorUi.Decimal',
		'Umbraco.Slider': 'Umb.PropertyEditorUi.Slider',

		// Date/time
		'Umbraco.DateTime': 'Umb.PropertyEditorUi.DatePicker',
		'Umbraco.DateOnly': 'Umb.PropertyEditorUi.DateOnlyPicker',
		'Umbraco.TimeOnly': 'Umb.PropertyEditorUi.TimeOnlyPicker',
		'Umbraco.DateTimeUnspecified': 'Umb.PropertyEditorUi.DateTimePicker',
		'Umbraco.DateTimeWithTimeZone': 'Umb.PropertyEditorUi.DateTimeWithTimeZonePicker',

		// Colors
		'Umbraco.ColorPicker': 'Umb.PropertyEditorUi.ColorPicker',
		'Umbraco.ColorPicker.EyeDropper': 'Umb.PropertyEditorUi.EyeDropper',

		// Block editors
		'Umbraco.BlockList': 'Umb.PropertyEditorUi.BlockList',
		'Umbraco.BlockGrid': 'Umb.PropertyEditorUi.BlockGrid',

		// Other
		'Umbraco.Tags': 'Umb.PropertyEditorUi.Tags',
		'Umbraco.EmailAddress': 'Umb.PropertyEditorUi.Email',
		'Umbraco.ListView': 'Umb.PropertyEditorUi.Collection',
		'Umbraco.Label': 'Umb.PropertyEditorUi.Label',
		'Umbraco.IconPicker': 'Umb.PropertyEditorUi.IconPicker',
		'Umbraco.MultipleTextstring': 'Umb.PropertyEditorUi.MultipleTextString',
	};

	return mapping[propertyEditorAlias] || `Umb.PropertyEditorUi.${propertyEditorAlias.replace('Umbraco.', '')}`;
}

/**
 * Format GUID to lowercase with hyphens
 */
export function formatGuid(guid: string): string {
	if (!guid) return guid;
	return guid.toLowerCase();
}

/**
 * Parse JSON config safely
 */
export function parseConfig(config: string | null): Record<string, unknown>[] {
	if (!config) return [];
	try {
		const parsed = JSON.parse(config);
		// Convert object config to array of { alias, value } pairs
		if (typeof parsed === 'object' && parsed !== null && !Array.isArray(parsed)) {
			return Object.entries(parsed).map(([alias, value]) => ({ alias, value }));
		}
		return [];
	} catch {
		return [];
	}
}

/**
 * Get content variation type from variations flag
 * 1 = varies by culture, 2 = varies by segment, 3 = varies by both
 */
export function getVariationFlags(variations: number): { variesByCulture: boolean; variesBySegment: boolean } {
	return {
		variesByCulture: (variations & 1) !== 0,
		variesBySegment: (variations & 2) !== 0,
	};
}

/**
 * Write a TypeScript data file
 */
export function writeDataFile(filename: string, content: string): void {
	const outputDir = getOutputDir();
	const outputPath = path.join(outputDir, filename);
	fs.writeFileSync(outputPath, content, 'utf-8');
	console.log(`Written: ${filename}`);
}
