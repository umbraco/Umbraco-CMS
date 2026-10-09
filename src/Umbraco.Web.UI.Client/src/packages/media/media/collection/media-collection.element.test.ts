import { UmbDropzoneMediaElement } from '../dropzone/dropzone-media.element.js';
import { UMB_MEDIA_COLLECTION_CONTEXT } from './media-collection.context-token.js';
import type { UmbMediaCollectionElement } from './media-collection.element.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbBooleanState, UmbNumberState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbDropzoneSubmittedEvent, UmbFileDropzoneItemStatus } from '@umbraco-cms/backoffice/dropzone';
import type { UmbUploadableItem } from '@umbraco-cms/backoffice/dropzone';
import type { ManifestCollectionView } from '@umbraco-cms/backoffice/collection';
import './media-collection.element.js';

@customElement('umb-test-media-collection-view')
class UmbTestMediaCollectionViewElement extends HTMLElement {}

type UmbPlaceholder = { unique: string; status: UmbFileDropzoneItemStatus; name?: string };

class UmbFakeMediaCollectionContext {
	#host: UmbControllerHostElement;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	getHostElement() {
		return this.#host;
	}

	#loading = new UmbBooleanState(false);
	loading = this.#loading.asObservable();

	#totalItems = new UmbNumberState(0);
	totalItems = this.#totalItems.asObservable();

	#filter = new UmbObjectState({});
	filter = this.#filter.asObservable();

	viewLayouts = new UmbArrayState([], (x) => x).asObservable();

	view = {
		views: new UmbArrayState<ManifestCollectionView>([], (x) => x.alias).asObservable(),
		currentView: new UmbObjectState<ManifestCollectionView | undefined>({
			type: 'collectionView',
			alias: 'Umb.Test.MediaCollectionView',
			name: 'Test Media Collection View',
			element: UmbTestMediaCollectionViewElement,
			meta: { label: 'Test', icon: 'icon-grid', pathName: 'test' },
		} as unknown as ManifestCollectionView).asObservable(),
		getCurrentView: () => ({ alias: 'Umb.Test.MediaCollectionView' }),
	};

	getEmptyLabel = () => 'empty';
	loadCollection = () => {};
	setFilter = () => {};

	setLoading(value: boolean) {
		this.#loading.setValue(value);
	}

	placeholderCalls: Array<Array<UmbPlaceholder>> = [];
	statusCalls: Array<{ unique: string; status?: UmbFileDropzoneItemStatus }> = [];
	progressCalls: Array<{ unique: string; progress: number }> = [];

	setPlaceholders(placeholders: Array<UmbPlaceholder>) {
		this.placeholderCalls.push(placeholders);
	}

	updatePlaceholderStatus(unique: string, status?: UmbFileDropzoneItemStatus) {
		this.statusCalls.push({ unique, status });
	}

	updatePlaceholderProgress(unique: string, progress: number) {
		this.progressCalls.push({ unique, progress });
	}
}

@customElement('umb-test-media-collection-host')
class UmbTestMediaCollectionHostElement extends UmbElementMixin(HTMLElement) {}

describe('UmbMediaCollectionElement', () => {
	let element: UmbMediaCollectionElement;
	let context: UmbFakeMediaCollectionContext;
	let dropzone: UmbDropzoneMediaElement;

	beforeEach(async () => {
		const host = await fixture<UmbTestMediaCollectionHostElement>(
			html`<umb-test-media-collection-host>
				<umb-media-collection></umb-media-collection>
			</umb-test-media-collection-host>`,
		);

		context = new UmbFakeMediaCollectionContext(host);
		host.provideContext(UMB_MEDIA_COLLECTION_CONTEXT, context as never);
		host.provideContext(UMB_ACTION_EVENT_CONTEXT, { getHostElement: () => host, dispatchEvent: () => true } as never);

		element = host.querySelector('umb-media-collection') as UmbMediaCollectionElement;

		// The toolbar holding the dropzone is only rendered once the first load has completed.
		context.setLoading(true);
		await aTimeout(0);
		context.setLoading(false);
		await aTimeout(0);
		await element.updateComplete;

		dropzone = element.shadowRoot!.querySelector('#dropzone') as UmbDropzoneMediaElement;
	});

	it('renders the dropzone', () => {
		expect(dropzone).to.be.instanceOf(UmbDropzoneMediaElement);
	});

	it('writes submitted dropzone items of the current parent to the collection context as placeholders', () => {
		const file = new File([''], 'photo.jpg');
		const items: Array<UmbUploadableItem> = [
			{
				unique: 'in-current-parent',
				parentUnique: null,
				status: UmbFileDropzoneItemStatus.WAITING,
				progress: 0,
				temporaryFile: { file, temporaryUnique: 'temp-1' },
			},
			{
				unique: 'in-other-parent',
				parentUnique: 'other-parent',
				status: UmbFileDropzoneItemStatus.WAITING,
				progress: 0,
				temporaryFile: { file, temporaryUnique: 'temp-2' },
			},
		];

		dropzone.dispatchEvent(new UmbDropzoneSubmittedEvent(items));

		expect(context.placeholderCalls).to.deep.equal([
			[{ unique: 'in-current-parent', status: UmbFileDropzoneItemStatus.WAITING, name: 'photo.jpg' }],
		]);
	});

	it('forwards the status and progress of dropzone items to the collection context', async () => {
		// The dropzone manager is driven directly so no upload request is made.
		// eslint-disable-next-line @typescript-eslint/no-explicit-any
		const manager = (dropzone as any)._manager;
		const [item] = manager._setupProgress({ files: [new File([''], 'photo.jpg')], folders: [] }, null) as Array<
			Required<UmbUploadableItem>
		>;

		item.temporaryFile.onProgress?.(50);
		manager._updateStatus(item, UmbFileDropzoneItemStatus.ERROR);
		await aTimeout(0);

		expect(context.progressCalls).to.deep.include({ unique: item.unique, progress: 50 });
		expect(context.statusCalls).to.deep.include({ unique: item.unique, status: UmbFileDropzoneItemStatus.ERROR });
	});
});
