import type { UmbMediaPathModel } from '../types.js';
import type { UmbMediaDetailModel } from '../../../types.js';
import { UmbMediaDetailRepository } from '../../../repository/index.js';
import { UmbMediaTreeRepository } from '../../../tree/index.js';
import { UMB_MEDIA_ROOT_ENTITY_TYPE } from '../../../entity.js';
import {
	css,
	html,
	customElement,
	nothing,
	state,
	repeat,
	property,
	type PropertyValues,
} from '@umbraco-cms/backoffice/external/lit';
import type { UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UmbMediaTypeStructureRepository, type UmbAllowedMediaTypeModel } from '@umbraco-cms/backoffice/media-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

const root: UmbMediaPathModel = { name: 'Media', unique: null, entityType: UMB_MEDIA_ROOT_ENTITY_TYPE };

@customElement('umb-media-picker-folder-path')
export class UmbMediaPickerFolderPathElement extends UmbLitElement {
	#mediaTreeRepository = new UmbMediaTreeRepository(this); // used to get file structure
	#mediaDetailRepository = new UmbMediaDetailRepository(this); // used to create folders
	#mediaTypeStructureRepository = new UmbMediaTypeStructureRepository(this);

	@property({ attribute: false })
	startNode?: UmbMediaPathModel = root;

	@property({ attribute: false })
	public set currentMedia(value: UmbMediaPathModel) {
		if (value !== this._currentMedia) {
			this._currentMedia = value;
			this.#loadPath();
		}
	}

	public get currentMedia() {
		return this._currentMedia;
	}

	@state()
	private _currentMedia: UmbMediaPathModel = root;

	@state()
	private _paths: Array<UmbMediaPathModel> = [root];

	@state()
	private _typingNewFolder = false;

	@state()
	private _allowedFolderTypes: Array<UmbAllowedMediaTypeModel> = [];

	@state()
	private _selectingFolderType = false;

	#selectedFolderType: UmbAllowedMediaTypeModel | null = null;

	// Cache for all folder media types to avoid repeated API calls
	#folderTypesPromise: Promise<UmbAllowedMediaTypeModel[]> | null = null;

	protected override firstUpdated(_changedProperties: PropertyValues): void {
		super.firstUpdated(_changedProperties);
		this.#loadPath();
	}

	async #loadPath() {
		const unique = this._currentMedia.unique;
		const entityType = this._currentMedia.entityType;

		const items = unique
			? (
					await this.#mediaTreeRepository.requestTreeItemAncestors({
						treeItem: { unique, entityType },
					})
				).data || []
			: [];

		const paths: Array<UmbMediaPathModel> = items.map((item) => ({
			name: item.name,
			unique: item.unique,
			entityType: item.entityType,
			mediaType: { unique: item.mediaType.unique },
		}));

		if (!this.startNode) {
			paths.unshift(root);
		}

		this._paths = [...paths];
		this.#updateAllowedFolderTypes();
	}

	async #getAllFolderTypes(): Promise<UmbAllowedMediaTypeModel[]> {
		if (!this.#folderTypesPromise) {
			this.#folderTypesPromise = this.#mediaTypeStructureRepository.requestMediaTypesOfFolders();
		}
		return this.#folderTypesPromise;
	}

	async #updateAllowedFolderTypes() {
		const currentPath = this._paths[this._paths.length - 1];
		const mediaTypeUnique = currentPath?.mediaType?.unique ?? null;
		const parentUnique = currentPath?.unique ?? null;

		// Fetch allowed children of the current parent's media type
		const { data: allowedChildrenData } = await this.#mediaTypeStructureRepository.requestAllowedChildrenOf(
			mediaTypeUnique,
			parentUnique,
		);
		const allowedChildren = allowedChildrenData?.items ?? [];

		// Fetch all folder media types
		const allFolderTypes = await this.#getAllFolderTypes();

		// Intersect: only folder types that are allowed children of this parent
		const allowedFolderTypeUniques = new Set(allowedChildren.map((c) => c.unique));
		this._allowedFolderTypes = allFolderTypes.filter((ft) => allowedFolderTypeUniques.has(ft.unique));
	}

	#goToFolder(entity: UmbMediaPathModel) {
		this._paths = [...this._paths].slice(0, this._paths.findIndex((path) => path.unique === entity.unique) + 1);
		this.currentMedia = entity;
		this.dispatchEvent(new UmbChangeEvent());
	}

	#focusFolderInput(folderType?: UmbAllowedMediaTypeModel) {
		if (folderType) {
			this.#selectedFolderType = folderType;
		}
		this._selectingFolderType = false;
		this._typingNewFolder = true;
		requestAnimationFrame(() => {
			const element = this.getHostElement().shadowRoot!.querySelector('#new-folder') as UUIInputElement;
			element.focus();
		});
	}

	#onAddFolderClick() {
		if (this._allowedFolderTypes.length === 1) {
			this.#selectedFolderType = this._allowedFolderTypes[0];
			this.#focusFolderInput();
		} else if (this._allowedFolderTypes.length > 1) {
			this._selectingFolderType = true;
		}
	}

	#cancelFolderTypeSelection() {
		this._selectingFolderType = false;
	}

	async #addFolder(e: UUIInputEvent) {
		e.stopPropagation();

		const newName = e.target.value as string;
		this._typingNewFolder = false;
		if (!newName || !this.#selectedFolderType?.unique) return;

		const newUnique = UmbId.new();
		const parentUnique = this._paths[this._paths.length - 1].unique;
		const folderTypeUnique = this.#selectedFolderType.unique;

		const preset: Partial<UmbMediaDetailModel> = {
			unique: newUnique,
			mediaType: {
				unique: folderTypeUnique,
				collection: null,
			},
			variants: [
				{
					culture: null,
					segment: null,
					name: newName,
					createDate: null,
					updateDate: null,
					flags: [],
				},
			],
		};
		const { data: scaffold } = await this.#mediaDetailRepository.createScaffold(preset);
		if (!scaffold) return;

		const { data } = await this.#mediaDetailRepository.create(scaffold, parentUnique);
		if (!data) return;

		const name = data.variants[0].name;
		const unique = data.unique;
		const entityType = data.entityType;
		const mediaType = { unique: folderTypeUnique };

		this._paths = [...this._paths, { name, unique, entityType, mediaType }];
		this.currentMedia = { name, unique, entityType, mediaType };
		this.dispatchEvent(new UmbChangeEvent());
	}

	#onKeypress(e: KeyboardEvent) {
		if (e.key === 'Enter') {
			requestAnimationFrame(() => {
				const element = this.getHostElement().shadowRoot!.querySelector('#new-folder') as UUIInputElement;
				element.blur();
			});
		}
	}

	override render() {
		return html`<div id="path">
			${repeat(
				this._paths,
				(path) => path.unique,
				(path) =>
					html`<uui-button
							compact
							.label=${path.name}
							?disabled=${this.currentMedia.unique === path.unique}
							@click=${() => this.#goToFolder(path)}></uui-button
						>/`,
			)}${this.#renderFolderCreation()}
		</div>`;
	}

	#renderFolderCreation() {
		if (this._typingNewFolder) {
			return html`<uui-input
				id="new-folder"
				label=${this.localize.term('create_enterFolderName')}
				placeholder=${this.localize.term('create_enterFolderName')}
				@blur=${this.#addFolder}
				@keypress=${this.#onKeypress}
				auto-width></uui-input>`;
		}

		if (this._selectingFolderType) {
			return html`<div id="folder-type-selection">
				${repeat(
					this._allowedFolderTypes,
					(ft) => ft.unique,
					(ft) =>
						html`<uui-button compact look="outline" .label=${ft.name} @click=${() => this.#focusFolderInput(ft)}>
							${ft.icon ? html`<umb-icon name=${ft.icon}></umb-icon>` : nothing} ${ft.name}
						</uui-button>`,
				)}
				<uui-button compact .label=${this.localize.term('general_cancel')} @click=${this.#cancelFolderTypeSelection}>
					<uui-icon name="icon-wrong"></uui-icon>
				</uui-button>
			</div>`;
		}

		if (this._allowedFolderTypes.length === 0) {
			return nothing;
		}

		return html`<uui-button
			label=${this.localize.term('visuallyHiddenTexts_createNewFolder')}
			compact
			@click=${this.#onAddFolderClick}>
			<uui-icon name="icon-add"></uui-icon>
		</uui-button>`;
	}

	static override styles = [
		css`
			#path {
				display: flex;
				align-items: center;
				margin: 0 var(--uui-size-3);
			}
			#path uui-button {
				font-weight: bold;
			}
			#path uui-input {
				height: 100%;
			}

			#new-folder {
				margin-left: var(--uui-size-2);
			}

			#path uui-button uui-icon {
				--uui-icon-color: inherit;
			}

			#folder-type-selection {
				display: flex;
				align-items: center;
				gap: var(--uui-size-2);
				margin-left: var(--uui-size-2);
			}

			#folder-type-selection uui-button {
				font-weight: normal;
			}

			#folder-type-selection umb-icon {
				margin-right: var(--uui-size-1);
			}
		`,
	];
}

export default UmbMediaPickerFolderPathElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-picker-folder-path': UmbMediaPickerFolderPathElement;
	}
}
