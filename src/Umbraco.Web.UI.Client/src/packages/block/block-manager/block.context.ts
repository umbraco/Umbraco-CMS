import type { UmbBlockTypeBase } from '../block-type/types.js';
import type { UmbBlockLayoutBaseModel, UmbBlockDataType } from './types.js';
import { UMB_BLOCK_MANAGER_CONTEXT, type UmbBlockManagerContext } from './block-manager.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';

export class UmbBlockContext<
	BlockType extends UmbBlockTypeBase = UmbBlockTypeBase,
	BlockLayoutType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel,
> extends UmbContextBase<UmbBlockContext> {
	//
	#manager?: UmbBlockManagerContext;

	#blockTypeName = new UmbStringState(undefined);
	public readonly blockTypeName = this.#blockTypeName.asObservable();

	#label = new UmbStringState('');
	public readonly label = this.#label.asObservable();

	#blockType = new UmbObjectState<BlockType | undefined>(undefined);
	public readonly blockType = this.#blockType.asObservable();
	public readonly blockTypeContentElementTypeKey = this.#blockType.asObservablePart((x) => x?.contentElementTypeKey);
	public readonly blockTypeSettingsElementTypeKey = this.#blockType.asObservablePart((x) => x?.settingsElementTypeKey);

	#layout = new UmbObjectState<BlockLayoutType | undefined>(undefined);
	public readonly layout = this.#layout.asObservable();
	public readonly contentUdi = this.#layout.asObservablePart((x) => x?.contentUdi);

	#content = new UmbObjectState<UmbBlockDataType | undefined>(undefined);
	public readonly content = this.#content.asObservable();
	public readonly contentTypeKey = this.#content.asObservablePart((x) => x?.contentTypeKey);

	#settings = new UmbObjectState<UmbBlockDataType | undefined>(undefined);
	public readonly settings = this.#settings.asObservable();

	/**
	 * Set the layout entry object.
	 * @method setLayout
	 * @param {BlockLayoutType | undefined} layout entry object.
	 * @returns {void}
	 */
	setLayout(layout: BlockLayoutType | undefined) {
		this.#layout.next(layout);
	}

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_CONTEXT);

		// Consume block manager:
		this.consumeContext(UMB_BLOCK_MANAGER_CONTEXT, (manager) => {
			this.#manager = manager;
			this.#observeBlockType();
			this.#observeData();
		});

		// Observe UDI:
		this.observe(this.contentUdi, (contentUdi) => {
			if (!contentUdi) return;
			this.#observeData();
		});

		// Observe contentElementTypeKey:
		this.observe(this.contentTypeKey, (contentElementTypeKey) => {
			if (!contentElementTypeKey) return;
			this.#observeBlockType();
		});

		// Observe blockType:
		this.observe(this.blockType, (blockType) => {
			if (!blockType) return;
			this.#observeBlockTypeLabel();
			this.#observeBlockTypeContentElementName();
		});
	}

	#observeData() {
		if (!this.#manager) return;
		const contentUdi = this.#layout.value?.contentUdi;
		if (!contentUdi) return;

		// observe content:
		this.observe(
			this.#manager.contentOf(contentUdi),
			(content) => {
				this.#content.next(content);
			},
			'observeContent',
		);

		// observe settings:
		const settingsUdi = this.#layout.value?.settingsUdi;
		if (settingsUdi) {
			this.observe(
				this.#manager.contentOf(settingsUdi),
				(content) => {
					this.#settings.next(content);
				},
				'observeSettings',
			);
		}
	}

	#observeBlockType() {
		if (!this.#manager) return;
		const contentTypeKey = this.#content.value?.contentTypeKey;
		if (!contentTypeKey) return;

		// observe blockType:
		this.observe(
			this.#manager.blockTypeOf(contentTypeKey),
			(blockType) => {
				this.#blockType.next(blockType as BlockType);
			},
			'observeBlockType',
		);
	}

	#observeBlockTypeLabel() {
		if (!this.#manager) return;
		const blockType = this.#blockType.value;
		if (!blockType) return;

		if (blockType.label) {
			this.removeControllerByAlias('observeContentTypeName');
			// Missing part for label syntax, as we need to store the syntax, interpretive it and then set the label: (here we are just parsing the label syntax)
			this.#label.next(blockType.label);
			return;
		} else {
			// TODO: Maybe this could be skipped if we had a fallback label which was set to get the content element type name?
			// Get the name of the content element type for label:
			this.observe(
				this.blockTypeName,
				(contentTypeName) => {
					this.#label.next(contentTypeName ?? 'no name');
				},
				'observeBlockTypeName',
			);
		}
	}

	#observeBlockTypeContentElementName() {
		if (!this.#manager) return;
		const contentElementTypeKey = this.#blockType.value?.contentElementTypeKey;
		if (!contentElementTypeKey) return;

		// observe blockType:
		this.observe(
			this.#manager.contentTypeNameOf(contentElementTypeKey),
			(contentTypeName) => {
				this.#blockTypeName.next(contentTypeName);
			},
			'observeBlockTypeContentElementTypeName',
		);
	}
}

export const UMB_BLOCK_CONTEXT = new UmbContextToken<UmbBlockContext, UmbBlockContext>('UmbBlockContext');
