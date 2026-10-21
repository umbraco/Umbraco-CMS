import { UmbMediaDetailRepository } from '../repository/detail/index.js';
import { UmbMediaDropzoneManager } from './media-dropzone.manager.js';
import { UMB_DROPZONE_MEDIA_TYPE_PICKER_MODAL } from './modals/index.js';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbFileDropzoneItemStatus, type UmbUploadableItem } from '@umbraco-cms/backoffice/dropzone';
import { UmbMediaTypeStructureRepository } from '@umbraco-cms/backoffice/media-type';
import { TemporaryFileStatus } from '@umbraco-cms/backoffice/temporary-file';

@customElement('test-media-dropzone-manager-host')
class UmbTestMediaDropzoneManagerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const IMAGE_TYPE = { unique: 'image', name: 'Image', icon: 'icon-picture' };
const PHOTO_TYPE = { unique: 'photo', name: 'Photo', icon: 'icon-picture' };
const FILE_TYPE = { unique: 'file', name: 'File', icon: 'icon-document' };

type PrototypeOverride = { target: object; key: PropertyKey; descriptor?: PropertyDescriptor };

interface TestState {
	host: UmbTestMediaDropzoneManagerHostElement;
	manager: UmbMediaDropzoneManager;
	pickerData: Array<any>;
	pickerValue: { mediaTypeUnique: string | undefined } | undefined;
	createdMediaTypes: Array<string>;
	overrides: Array<PrototypeOverride>;
}

const override = (state: TestState, target: object, key: PropertyKey, value: unknown) => {
	state.overrides.push({ target, key, descriptor: Object.getOwnPropertyDescriptor(target, key) });
	Object.defineProperty(target, key, { configurable: true, writable: true, value });
};

const restoreOverrides = (state: TestState) => {
	for (const { target, key, descriptor } of state.overrides.reverse()) {
		if (descriptor) Object.defineProperty(target, key, descriptor);
		else delete (target as Record<PropertyKey, unknown>)[key];
	}
};

const provideModalManager = (state: TestState) => {
	const modalManager = {
		getHostElement: () => state.host,
		open: (_host: unknown, token: unknown, args: { data: any }) => {
			expect(token).to.equal(UMB_DROPZONE_MEDIA_TYPE_PICKER_MODAL);
			state.pickerData.push(args.data);
			return {
				onSubmit: () =>
					state.pickerValue ? Promise.resolve(state.pickerValue) : Promise.reject(new Error('Cancelled')),
			};
		},
	};
	const provider = new UmbContextProvider(
		state.host,
		UMB_MODAL_MANAGER_CONTEXT,
		modalManager as unknown as typeof UMB_MODAL_MANAGER_CONTEXT.TYPE,
	);
	provider.hostConnected();
};

const mediaTypesForExtension = (fileExtension: string) => {
	if (fileExtension === 'png') {
		return [
			{ ...IMAGE_TYPE, matchedFileExtension: true },
			{ ...PHOTO_TYPE, matchedFileExtension: true },
			{ ...FILE_TYPE, matchedFileExtension: false },
		];
	}
	if (fileExtension === 'svg') {
		return [
			{ ...PHOTO_TYPE, matchedFileExtension: true },
			{ ...FILE_TYPE, matchedFileExtension: true },
		];
	}
	return [{ ...FILE_TYPE, matchedFileExtension: false }];
};

const mockRepositories = (state: TestState) => {
	override(state, UmbMediaTypeStructureRepository.prototype, 'requestAllAllowedChildrenOf', async () => ({
		data: { items: [IMAGE_TYPE, PHOTO_TYPE, FILE_TYPE] },
	}));
	override(state, UmbMediaTypeStructureRepository.prototype, 'requestMediaTypesOf', async ({ fileExtension }: any) =>
		mediaTypesForExtension(fileExtension),
	);
	override(state, UmbMediaDetailRepository.prototype, 'createScaffold', async (preset: any) => ({ data: preset }));
	override(state, UmbMediaDetailRepository.prototype, 'createSilently', async (model: any) => {
		state.createdMediaTypes.push(model.mediaType.unique);
		return { data: model };
	});
};

const createManager = (host: UmbTestMediaDropzoneManagerHostElement) => {
	const manager = new UmbMediaDropzoneManager(host);
	(manager as any)._tempFileManager.uploadOne = async (temporaryFile: unknown) => ({
		...(temporaryFile as object),
		status: TemporaryFileStatus.SUCCESS,
	});
	return manager;
};

describe('UmbMediaDropzoneManager', () => {
	let state: TestState;

	const waitForUpload = async (items: Array<UmbUploadableItem>) => {
		let progressItems: Array<UmbUploadableItem> = items;
		const subscription = state.manager.progressItems.subscribe((value) => (progressItems = value));
		for (let i = 0; i < 100 && progressItems.some((x) => x.status === UmbFileDropzoneItemStatus.WAITING); i++) {
			await aTimeout(0);
		}
		subscription.unsubscribe();
		return progressItems;
	};

	beforeEach(() => {
		const host = new UmbTestMediaDropzoneManagerHostElement();
		document.body.appendChild(host);
		state = {
			host,
			manager: undefined as unknown as UmbMediaDropzoneManager,
			pickerData: [],
			pickerValue: { mediaTypeUnique: PHOTO_TYPE.unique },
			createdMediaTypes: [],
			overrides: [],
		};
		provideModalManager(state);
		mockRepositories(state);
		state.manager = createManager(host);
	});

	afterEach(() => {
		state.manager.destroy();
		restoreOverrides(state);
		document.body.innerHTML = '';
	});
	it('asks once for a media type when multiple files share the same choices', async () => {
		const files = [new File(['a'], 'a.png'), new File(['b'], 'b.png')];

		const items = state.manager.createMediaItems({ files, folders: [] });
		const progressItems = await waitForUpload(items);

		expect(state.pickerData).to.have.lengthOf(1);
		expect(state.pickerData[0].options.map((x: { unique: string }) => x.unique)).to.deep.equal([
			IMAGE_TYPE.unique,
			PHOTO_TYPE.unique,
		]);
		expect(state.createdMediaTypes).to.deep.equal([PHOTO_TYPE.unique, PHOTO_TYPE.unique]);
		expect(progressItems.map((x) => x.status)).to.deep.equal([
			UmbFileDropzoneItemStatus.COMPLETE,
			UmbFileDropzoneItemStatus.COMPLETE,
		]);
	});

	it('asks separately for files that offer different media type choices', async () => {
		const files = [new File(['a'], 'a.png'), new File(['b'], 'b.svg'), new File(['c'], 'c.png')];

		const items = state.manager.createMediaItems({ files, folders: [] });
		await waitForUpload(items);

		expect(state.pickerData).to.have.lengthOf(2);
		expect(state.createdMediaTypes).to.have.lengthOf(3);
	});

	it('does not ask when the files have no competing specific media types', async () => {
		const files = [new File(['a'], 'a.txt'), new File(['b'], 'b.txt')];

		const items = state.manager.createMediaItems({ files, folders: [] });
		await waitForUpload(items);

		expect(state.pickerData).to.have.lengthOf(0);
		expect(state.createdMediaTypes).to.deep.equal([FILE_TYPE.unique, FILE_TYPE.unique]);
	});

	it('cancels all files that share a cancelled media type choice', async () => {
		state.pickerValue = undefined;
		const files = [new File(['a'], 'a.png'), new File(['b'], 'b.png')];

		const items = state.manager.createMediaItems({ files, folders: [] });
		const progressItems = await waitForUpload(items);

		expect(state.pickerData).to.have.lengthOf(1);
		expect(state.createdMediaTypes).to.have.lengthOf(0);
		expect(progressItems.map((x) => x.status)).to.deep.equal([
			UmbFileDropzoneItemStatus.CANCELLED,
			UmbFileDropzoneItemStatus.CANCELLED,
		]);
	});

	it('names every file a media type choice applies to', async () => {
		const files = [new File(['a'], 'a.png'), new File(['b'], 'b.svg'), new File(['c'], 'c.png')];

		const items = state.manager.createMediaItems({ files, folders: [] });
		await waitForUpload(items);

		expect(state.pickerData.map((data) => data.files.map((file: File) => file.name))).to.deep.equal([
			['a.png', 'c.png'],
			['b.svg'],
		]);
	});
});
