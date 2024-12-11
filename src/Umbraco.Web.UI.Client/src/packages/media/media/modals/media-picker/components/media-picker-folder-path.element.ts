import type { UmbMediaPathModel } from '../types.js';
import type { UmbMediaDetailModel } from '../../../types.js';
import { UmbMediaDetailRepository } from '../../../repository/index.js';
import { UmbMediaTreeRepository } from '../../../tree/index.js';
import { UMB_MEDIA_ROOT_ENTITY_TYPE } from '../../../entity.js';
import {
	css,
	html,
	customElement,
	state,
	repeat,
	property,
	type PropertyValues,
} from '@umbraco-cms/backoffice/external/lit';
import type { UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { getUmbracoFolderUnique } from '@umbraco-cms/backoffice/media-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

const root: UmbMediaPathModel = { name: 'Media', unique: null, entityType: UMB_MEDIA_ROOT_ENTITY_TYPE };

@customElement('umb-media-picker-folder-path')
export class UmbMediaPickerFolderPathElement extends UmbLitElement {
	#mediaTreeRepository = new UmbMediaTreeRepository(this); // used to get file structure
	#mediaDetailRepository = new UmbMediaDetailRepository(this); // used to create folders

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
		}));

		if (!this.startNode) {
			paths.unshift(root);
		}

		this._paths = [...paths];
	}

	#goToFolder(entity: UmbMediaPathModel) {
		this._paths = [...this._paths].slice(0, this._paths.findIndex((path) => path.unique === entity.unique) + 1);
		this.currentMedia = entity;
		this.dispatchEvent(new UmbChangeEvent());
	}

	#focusFolderInput() {
		this._typingNewFolder = true;
		requestAnimationFrame(() => {
			const element = this.getHostElement().shadowRoot!.querySelector('#new-folder') as UUIInputElement;
			element.focus();
			element.select();
		});
	}

	async #addFolder(e: UUIInputEvent) {
		e.stopPropagation();

		const newName = e.target.value as string;
		this._typingNewFolder = false;
		if (!newName) return;

		const newUnique = UmbId.new();
		const parentUnique = this._paths[this._paths.length - 1].unique;

		const preset: Partial<UmbMediaDetailModel> = {
			unique: newUnique,
			mediaType: {
				unique: getUmbracoFolderUnique(),
				collection: null,
			},
			variants: [
				{
					culture: null,
					segment: null,
					name: newName,
					createDate: null,
					updateDate: null,
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

		this._paths = [...this._paths, { name, unique, entityType }];
		this.currentMedia = { name, unique, entityType };
		this.dispatchEvent(new UmbChangeEvent());
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
			)}${this._typingNewFolder
				? html`<uui-input
						id="new-folder"
						label="enter a name"
						value="new folder name"
						@blur=${this.#addFolder}
						auto-width></uui-input>`
				: html`<uui-button label="add folder" compact @click=${this.#focusFolderInput}>+</uui-button>`}
		</div>`;
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
		`,
	];
}

export default UmbMediaPickerFolderPathElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-picker-folder-path': UmbMediaPickerFolderPathElement;
	}
}
