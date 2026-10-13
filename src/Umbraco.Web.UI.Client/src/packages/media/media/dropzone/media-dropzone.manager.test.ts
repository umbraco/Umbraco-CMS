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

describe('UmbMediaDropzoneManager', () => {
	let host: UmbTestMediaDropzoneManagerHostElement;
	let manager: UmbMediaDropzoneManager;
	let pickerData: Array<any>;
	let pickerValue: { mediaTypeUnique: string | undefined } | undefined;
	let createdMediaTypes: Array<string>;
	let overrides: Array<PrototypeOverride>;

	const override = (target: object, key: PropertyKey, value: unknown) => {
		overrides.push({ target, key, descriptor: Object.getOwnPropertyDescriptor(target, key) });
		Object.defineProperty(target, key, { configurable: true, writable: true, value });
	};

	const waitForUpload = async (items: Array<UmbUploadableItem>) => {
		let progressItems: Array<UmbUploadableItem> = items;
		const subscription = manager.progressItems.subscribe((value) => (progressItems = value));
		for (let i = 0; i < 100 && progressItems.some((x) => x.status === UmbFileDropzoneItemStatus.WAITING); i++) {
			await aTimeout(0);
		}
		subscription.unsubscribe();
		return progressItems;
	};

	beforeEach(() => {
		host = new UmbTestMediaDropzoneManagerHostElement();
		document.body.appendChild(host);
		pickerData = [];
		pickerValue = { mediaTypeUnique: PHOTO_TYPE.unique };
		createdMediaTypes = [];
		overrides = [];

		const modalManager = {
			getHostElement: () => host,
			open: (_host: unknown, token: unknown, args: { data: any }) => {
				expect(token).to.equal(UMB_DROPZONE_MEDIA_TYPE_PICKER_MODAL);
				pickerData.push(args.data);
				return {
					onSubmit: () => (pickerValue ? Promise.resolve(pickerValue) : Promise.reject(new Error('Cancelled'))),
				};
			},
		};
		const provider = new UmbContextProvider(
			host,
			UMB_MODAL_MANAGER_CONTEXT,
			modalManager as unknown as typeof UMB_MODAL_MANAGER_CONTEXT.TYPE,
		);
		provider.hostConnected();

		override(UmbMediaTypeStructureRepository.prototype, 'requestAllowedChildrenOf', async () => ({
			data: { items: [IMAGE_TYPE, PHOTO_TYPE, FILE_TYPE] },
		}));
		override(UmbMediaTypeStructureRepository.prototype, 'requestMediaTypesOf', async ({ fileExtension }: any) =>
			fileExtension === 'png'
				? [
						{ ...IMAGE_TYPE, matchedFileExtension: true },
						{ ...PHOTO_TYPE, matchedFileExtension: true },
						{ ...FILE_TYPE, matchedFileExtension: false },
					]
				: fileExtension === 'svg'
					? [
							{ ...PHOTO_TYPE, matchedFileExtension: true },
							{ ...FILE_TYPE, matchedFileExtension: true },
						]
					: [{ ...FILE_TYPE, matchedFileExtension: false }],
		);
		override(UmbMediaDetailRepository.prototype, 'createScaffold', async (preset: any) => ({ data: preset }));
		override(UmbMediaDetailRepository.prototype, 'createSilently', async (model: any) => {
			createdMediaTypes.push(model.mediaType.unique);
			return { data: model };
		});

		manager = new UmbMediaDropzoneManager(host);
		(manager as any)._tempFileManager.uploadOne = async (temporaryFile: unknown) => ({
			...(temporaryFile as object),
			status: TemporaryFileStatus.SUCCESS,
		});
	});

	afterEach(() => {
		manager.destroy();
		for (const { target, key, descriptor } of overrides.reverse()) {
			if (descriptor) Object.defineProperty(target, key, descriptor);
			else delete (target as Record<PropertyKey, unknown>)[key];
		}
		document.body.innerHTML = '';
	});

	it('asks once for a media type when multiple files share the same choices', async () => {
		const files = [new File(['a'], 'a.png'), new File(['b'], 'b.png')];

		const items = manager.createMediaItems({ files, folders: [] });
		const progressItems = await waitForUpload(items);

		expect(pickerData).to.have.lengthOf(1);
		expect(pickerData[0].options.map((x: { unique: string }) => x.unique)).to.deep.equal([
			IMAGE_TYPE.unique,
			PHOTO_TYPE.unique,
		]);
		expect(createdMediaTypes).to.deep.equal([PHOTO_TYPE.unique, PHOTO_TYPE.unique]);
		expect(progressItems.map((x) => x.status)).to.deep.equal([
			UmbFileDropzoneItemStatus.COMPLETE,
			UmbFileDropzoneItemStatus.COMPLETE,
		]);
	});

	it('asks separately for files that offer different media type choices', async () => {
		const files = [new File(['a'], 'a.png'), new File(['b'], 'b.svg'), new File(['c'], 'c.png')];

		const items = manager.createMediaItems({ files, folders: [] });
		await waitForUpload(items);

		expect(pickerData).to.have.lengthOf(2);
		expect(createdMediaTypes).to.have.lengthOf(3);
	});

	it('does not ask when the files have no competing specific media types', async () => {
		const files = [new File(['a'], 'a.txt'), new File(['b'], 'b.txt')];

		const items = manager.createMediaItems({ files, folders: [] });
		await waitForUpload(items);

		expect(pickerData).to.have.lengthOf(0);
		expect(createdMediaTypes).to.deep.equal([FILE_TYPE.unique, FILE_TYPE.unique]);
	});

	it('cancels all files that share a cancelled media type choice', async () => {
		pickerValue = undefined;
		const files = [new File(['a'], 'a.png'), new File(['b'], 'b.png')];

		const items = manager.createMediaItems({ files, folders: [] });
		const progressItems = await waitForUpload(items);

		expect(pickerData).to.have.lengthOf(1);
		expect(createdMediaTypes).to.have.lengthOf(0);
		expect(progressItems.map((x) => x.status)).to.deep.equal([
			UmbFileDropzoneItemStatus.CANCELLED,
			UmbFileDropzoneItemStatus.CANCELLED,
		]);
	});
});
