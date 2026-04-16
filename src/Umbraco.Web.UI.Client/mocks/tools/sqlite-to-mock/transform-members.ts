/**
 * Transform member records into mock data format.
 */
import { prepare, ObjectTypes, formatGuid, writeDataFile } from './db.js';

interface MemberRow {
	nodeId: number;
	uniqueId: string;
	parentId: number;
	level: number;
	path: string;
	sortOrder: number;
	trashed: number;
	text: string | null;
	createDate: string;
	contentTypeNodeId: number;
	contentTypeAlias: string;
	contentTypeIcon: string | null;
	listView: string | null;
}

interface CmsMember {
	nodeId: number;
	Email: string;
	LoginName: string;
	isApproved: number;
	isLockedOut: number;
	failedPasswordAttempts: number | null;
	lastLoginDate: string | null;
	lastLockoutDate: string | null;
	lastPasswordChangeDate: string | null;
}

interface Member2MemberGroup {
	Member: number;
	MemberGroup: number;
}

interface ContentVersion {
	id: number;
	nodeId: number;
	versionDate: string;
	userId: number | null;
	current: number;
	text: string | null;
}

interface PropertyData {
	versionId: number;
	propertyTypeId: number;
	languageId: number | null;
	segment: string | null;
	intValue: number | null;
	decimalValue: number | null;
	dateValue: string | null;
	varcharValue: string | null;
	textValue: string | null;
}

interface PropertyTypeInfo {
	id: number;
	alias: string;
	contentTypeId: number;
	editorAlias: string;
}

export function transformMembers(): void {
	// Query members with their content type information
	const query = prepare(`
		SELECT
			n.id as nodeId, n.uniqueId, n.parentId, n.level, n.path, n.sortOrder,
			n.trashed, n.text, n.createDate,
			c.contentTypeId as contentTypeNodeId,
			ct.alias as contentTypeAlias,
			ct.icon as contentTypeIcon,
			ct.listView
		FROM umbracoNode n
		JOIN umbracoContent c ON n.id = c.nodeId
		JOIN cmsContentType ct ON c.contentTypeId = ct.nodeId
		WHERE n.nodeObjectType = ?
		ORDER BY n.level, n.sortOrder
	`);

	const rows = query.all(ObjectTypes.Member) as MemberRow[];

	// Get member-specific auth data
	const memberQuery = prepare(`
		SELECT nodeId, Email, LoginName, isApproved, isLockedOut,
		       failedPasswordAttempts, lastLoginDate, lastLockoutDate, lastPasswordChangeDate
		FROM cmsMember
	`);

	const memberData = memberQuery.all() as CmsMember[];
	const memberDataMap = new Map<number, CmsMember>();
	for (const m of memberData) {
		memberDataMap.set(m.nodeId, m);
	}

	// Get member-to-group relationships
	const groupMembershipQuery = prepare(`
		SELECT Member, MemberGroup
		FROM cmsMember2MemberGroup
	`);

	const memberships = groupMembershipQuery.all() as Member2MemberGroup[];
	const memberGroupsByMember = new Map<number, number[]>();
	for (const m of memberships) {
		const list = memberGroupsByMember.get(m.Member) || [];
		list.push(m.MemberGroup);
		memberGroupsByMember.set(m.Member, list);
	}

	// Get member group nodes for ID lookup (nodeId -> GUID)
	const memberGroupNodeQuery = prepare(`
		SELECT id, uniqueId
		FROM umbracoNode
		WHERE nodeObjectType = ?
	`);

	const memberGroupNodes = memberGroupNodeQuery.all(ObjectTypes.MemberGroup) as { id: number; uniqueId: string }[];
	const memberGroupIdMap = new Map<number, string>();
	for (const mg of memberGroupNodes) {
		memberGroupIdMap.set(mg.id, formatGuid(mg.uniqueId));
	}

	// Get current content versions
	const versionQuery = prepare(`
		SELECT id, nodeId, versionDate, userId, current, text
		FROM umbracoContentVersion
		WHERE current = 1
	`);

	const versions = versionQuery.all() as ContentVersion[];
	const versionMap = new Map<number, ContentVersion>();
	for (const v of versions) {
		versionMap.set(v.nodeId, v);
	}

	// Get property data for current versions
	const propertyDataQuery = prepare(`
		SELECT
			pd.versionId, pd.propertyTypeId, pd.languageId, pd.segment,
			pd.intValue, pd.decimalValue, pd.dateValue, pd.varcharValue, pd.textValue
		FROM umbracoPropertyData pd
		JOIN umbracoContentVersion cv ON pd.versionId = cv.id
		WHERE cv.current = 1
	`);

	const propertyData = propertyDataQuery.all() as PropertyData[];
	const propertyDataByVersion = new Map<number, PropertyData[]>();
	for (const pd of propertyData) {
		const list = propertyDataByVersion.get(pd.versionId) || [];
		list.push(pd);
		propertyDataByVersion.set(pd.versionId, list);
	}

	// Get property type information
	const propertyTypeQuery = prepare(`
		SELECT
			pt.id, pt.Alias as alias, pt.contentTypeId,
			dt.propertyEditorAlias as editorAlias
		FROM cmsPropertyType pt
		JOIN umbracoDataType dt ON pt.dataTypeId = dt.nodeId
	`);

	const propertyTypes = propertyTypeQuery.all() as PropertyTypeInfo[];
	const propertyTypeMap = new Map<number, PropertyTypeInfo>();
	for (const pt of propertyTypes) {
		propertyTypeMap.set(pt.id, pt);
	}

	// Get member type nodes for ID lookup
	const memberTypeNodeQuery = prepare(`
		SELECT id, uniqueId
		FROM umbracoNode
		WHERE nodeObjectType = ?
	`);

	const memberTypeNodes = memberTypeNodeQuery.all(ObjectTypes.MemberType) as { id: number; uniqueId: string }[];
	const memberTypeIdMap = new Map<number, string>();
	for (const mt of memberTypeNodes) {
		memberTypeIdMap.set(mt.id, formatGuid(mt.uniqueId));
	}

	// Transform members
	const members = rows
		.filter((row) => row.trashed === 0)
		.map((row) => {
			const cmsMember = memberDataMap.get(row.nodeId);
			const version = versionMap.get(row.nodeId);

			// Resolve member group GUIDs
			const groupNodeIds = memberGroupsByMember.get(row.nodeId) || [];
			const groups = groupNodeIds
				.map((nodeId) => memberGroupIdMap.get(nodeId))
				.filter((id): id is string => id !== undefined);

			// Get property values for this member's version
			const versionPropertyData = version ? propertyDataByVersion.get(version.id) || [] : [];

			// Transform property values
			const values = versionPropertyData
				.map((pd) => {
					const propType = propertyTypeMap.get(pd.propertyTypeId);
					if (!propType) return null;

					// Determine the value based on what's populated
					let value: unknown = null;
					if (pd.textValue !== null) {
						try {
							value = JSON.parse(pd.textValue);
						} catch {
							value = pd.textValue;
						}
					} else if (pd.varcharValue !== null) {
						value = pd.varcharValue;
					} else if (pd.intValue !== null) {
						value = pd.intValue;
					} else if (pd.decimalValue !== null) {
						value = pd.decimalValue;
					} else if (pd.dateValue !== null) {
						value = pd.dateValue;
					}

					return {
						editorAlias: propType.editorAlias,
						alias: propType.alias,
						value,
					};
				})
				.filter((v): v is NonNullable<typeof v> => v !== null);

			// Build member type reference
			const memberTypeInfo: Record<string, unknown> = {
				id: memberTypeIdMap.get(row.contentTypeNodeId) || `unknown-${row.contentTypeNodeId}`,
				icon: row.contentTypeIcon || 'icon-user',
			};

			if (row.listView) {
				memberTypeInfo.collection = { id: row.listView.toLowerCase() };
			}

			return {
				id: formatGuid(row.uniqueId),
				email: cmsMember?.Email || '',
				username: cmsMember?.LoginName || '',
				isApproved: cmsMember?.isApproved === 1,
				isLockedOut: cmsMember?.isLockedOut === 1,
				isTwoFactorEnabled: false,
				failedPasswordAttempts: Number(cmsMember?.failedPasswordAttempts) || 0,
				lastLoginDate: cmsMember?.lastLoginDate || null,
				lastLockoutDate: cmsMember?.lastLockoutDate || null,
				lastPasswordChangeDate: cmsMember?.lastPasswordChangeDate || null,
				memberType: memberTypeInfo,
				groups,
				kind: 'Default',
				values,
				variants: [
					{
						culture: null,
						segment: null,
						name: row.text || 'Unnamed',
						createDate: row.createDate,
						updateDate: version?.versionDate || row.createDate,
					},
				],
				flags: [] as string[],
			};
		});

	// Generate TypeScript content
	const content = `import type { UmbMockMemberModel } from '../../mock-data-set.types.js';
import { MemberKindModel } from '@umbraco-cms/backoffice/external/backend-api';

const rawData = ${JSON.stringify(members, null, '\t')};

export const data: Array<UmbMockMemberModel> = rawData.map(member => ({
	...member,
	kind: MemberKindModel.DEFAULT,
}));
`;

	writeDataFile('member.data.ts', content);
	console.log(`Transformed ${members.length} members`);
}
