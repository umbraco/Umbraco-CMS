/**
 * Transform member group records into mock data format.
 */
import { prepare, ObjectTypes, formatGuid, writeDataFile } from './db.js';

interface MemberGroupRow {
	id: number;
	uniqueId: string;
	text: string | null;
}

export function transformMemberGroups(): void {
	const query = prepare(`
		SELECT id, uniqueId, text
		FROM umbracoNode
		WHERE nodeObjectType = ?
		  AND trashed = 0
		ORDER BY sortOrder
	`);

	const rows = query.all(ObjectTypes.MemberGroup) as MemberGroupRow[];

	const memberGroups = rows.map((row) => ({
		id: formatGuid(row.uniqueId),
		name: row.text || 'Unnamed',
		flags: [] as string[],
	}));

	const content = `import type { UmbMockMemberGroupModel } from '../../mock-data-set.types.js';

export const data: Array<UmbMockMemberGroupModel> = ${JSON.stringify(memberGroups, null, '\t')};
`;

	writeDataFile('member-group.data.ts', content);
	console.log(`Transformed ${memberGroups.length} member groups`);
}
