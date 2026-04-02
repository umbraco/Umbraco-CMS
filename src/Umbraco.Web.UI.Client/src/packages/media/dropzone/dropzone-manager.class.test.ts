import { UmbDropzoneManager } from './dropzone-manager.class.js';
import { UmbFileDropzoneItemStatus } from './constants.js';
import type { UmbUploadableItem } from './types.js';
import { expect } from '@open-wc/testing';
import type { UUIFileFolder } from '@umbraco-cms/backoffice/external/uui';

// Option B: plain mock host — avoids needing a registered custom element.
function createHost() {
	const controllers = new Set<any>();
	return {
		addUmbController: (c: any) => controllers.add(c),
		removeUmbController: (c: any) => controllers.delete(c),
		getHostElement: () => document.createElement('div'),
		hasUmbController: () => false,
	} as any;
}

// Helper: call the protected _setupProgress via any-cast
function setupProgress(
	manager: UmbDropzoneManager,
	items: { files: File[]; folders: UUIFileFolder[] },
	parent: string | null,
): UmbUploadableItem[] {
	return (manager as any)._setupProgress(items, parent);
}

function makeFolder(name: string, children: { files?: File[]; folders?: UUIFileFolder[] } = {}): UUIFileFolder {
	return {
		folderName: name,
		files: children.files ?? [],
		folders: children.folders ?? [],
	};
}

describe('UmbDropzoneManager._setupProgress', () => {
	let manager: UmbDropzoneManager;

	beforeEach(() => {
		const host = createHost();
		manager = new UmbDropzoneManager(host);
	});

	it('returns a folder item for each root folder', () => {
		const items = setupProgress(
			manager,
			{ files: [], folders: [makeFolder('A'), makeFolder('B')] },
			null,
		);
		const folderItems = items.filter((i) => i.folder);
		expect(folderItems.length).to.equal(2);
		expect(folderItems[0].folder!.name).to.equal('A');
		expect(folderItems[1].folder!.name).to.equal('B');
	});

	it('root folder items have parentUnique matching the passed parent', () => {
		const items = setupProgress(
			manager,
			{ files: [], folders: [makeFolder('A')] },
			'parent-123',
		);
		const folder = items.find((i) => i.folder);
		expect(folder!.parentUnique).to.equal('parent-123');
	});

	it('root file items have parentUnique matching the passed parent', () => {
		const file = new File([''], 'photo.jpg', { type: 'image/jpeg' });
		const items = setupProgress(manager, { files: [file], folders: [] }, null);
		expect(items.length).to.equal(1);
		expect(items[0].parentUnique).to.equal(null);
	});

	it('places each folder item before its children in the flat list', () => {
		const child = makeFolder('child', { files: [new File([''], 'x.txt')] });
		const parent = makeFolder('parent', { folders: [child] });
		const items = setupProgress(manager, { files: [], folders: [parent] }, null);

		const parentIdx = items.findIndex((i) => i.folder?.name === 'parent');
		const childIdx = items.findIndex((i) => i.folder?.name === 'child');
		const fileIdx = items.findIndex((i) => i.temporaryFile);

		expect(parentIdx).to.be.greaterThan(-1);
		expect(childIdx).to.be.greaterThan(parentIdx);
		expect(fileIdx).to.be.greaterThan(childIdx);
	});

	it('sets child parentUnique to the parent folder unique', () => {
		const child = makeFolder('child');
		const parent = makeFolder('parent', { folders: [child] });
		const items = setupProgress(manager, { files: [], folders: [parent] }, null);

		const parentItem = items.find((i) => i.folder?.name === 'parent');
		const childItem = items.find((i) => i.folder?.name === 'child');

		expect(childItem!.parentUnique).to.equal(parentItem!.unique);
	});

	it('sets folder item status to WAITING and progress to 100', () => {
		const items = setupProgress(manager, { files: [], folders: [makeFolder('A')] }, null);
		const folder = items.find((i) => i.folder);
		expect(folder!.status).to.equal(UmbFileDropzoneItemStatus.WAITING);
		expect(folder!.progress).to.equal(100);
	});

	it('sets file item status to WAITING and progress to 0', () => {
		const file = new File([''], 'doc.txt');
		const items = setupProgress(manager, { files: [file], folders: [] }, null);
		expect(items[0].status).to.equal(UmbFileDropzoneItemStatus.WAITING);
		expect(items[0].progress).to.equal(0);
	});
});
