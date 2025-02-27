import type { UmbUserGroupDetailModel } from '../../types.js';
import { UMB_USER_GROUP_ROOT_WORKSPACE_PATH } from '../../paths.js';
import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from './user-group-workspace.context-token.js';
import type { UUIBooleanInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { css, html, nothing, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbInputDocumentElement } from '@umbraco-cms/backoffice/document';
import type { UmbInputSectionElement } from '@umbraco-cms/backoffice/section';
import type { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbInputMediaElement } from '@umbraco-cms/backoffice/media';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbInputLanguageElement } from '@umbraco-cms/backoffice/language';
import { UMB_ICON_PICKER_MODAL } from '@umbraco-cms/backoffice/icon';
import type { UmbInputWithAliasElement } from '@umbraco-cms/backoffice/components';

// import of local components
import './components/user-group-entity-user-permission-list.element.js';
import './components/user-group-granular-permission-list.element.js';

@customElement('umb-user-group-workspace-editor')
export class UmbUserGroupWorkspaceEditorElement extends UmbLitElement {
	@state()
	private _isNew?: boolean = false;

	@state()
	private _unique?: UmbUserGroupDetailModel['unique'];

	@state()
	private _name?: UmbUserGroupDetailModel['name'];

	@state()
	private _alias?: UmbUserGroupDetailModel['alias'];

	@state()
	private _aliasCanBeChanged?: UmbUserGroupDetailModel['aliasCanBeChanged'] = true;

	@state()
	private _icon: UmbUserGroupDetailModel['icon'] = null;

	@state()
	private _sections: UmbUserGroupDetailModel['sections'] = [];

	@state()
	private _languages: UmbUserGroupDetailModel['languages'] = [];

	@state()
	private _hasAccessToAllLanguages: UmbUserGroupDetailModel['hasAccessToAllLanguages'] = false;

	@state()
	private _documentStartNode?: UmbUserGroupDetailModel['documentStartNode'];

	@state()
	private _documentRootAccess: UmbUserGroupDetailModel['documentRootAccess'] = false;

	@state()
	private _mediaStartNode?: UmbUserGroupDetailModel['mediaStartNode'];

	@state()
	private _mediaRootAccess: UmbUserGroupDetailModel['mediaRootAccess'] = false;

	#workspaceContext?: typeof UMB_USER_GROUP_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_USER_GROUP_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#observeUserGroup();
		});
	}

	#observeUserGroup() {
		if (!this.#workspaceContext) return;
		this.observe(this.#workspaceContext.isNew, (value) => (this._isNew = value), '_observeIsNew');
		this.observe(this.#workspaceContext.unique, (value) => (this._unique = value ?? undefined), '_observeUnique');
		this.observe(this.#workspaceContext.name, (value) => (this._name = value), '_observeName');
		this.observe(this.#workspaceContext.alias, (value) => (this._alias = value), '_observeAlias');
		this.observe(
			this.#workspaceContext.aliasCanBeChanged,
			(value) => (this._aliasCanBeChanged = value),
			'_observeAliasCanBeChanged',
		);
		this.observe(this.#workspaceContext.icon, (value) => (this._icon = value), '_observeIcon');
		this.observe(this.#workspaceContext.sections, (value) => (this._sections = value), '_observeSections');
		this.observe(this.#workspaceContext.languages, (value) => (this._languages = value), '_observeLanguages');
		this.observe(
			this.#workspaceContext.hasAccessToAllLanguages,
			(value) => (this._hasAccessToAllLanguages = value),
			'_observeHasAccessToAllLanguages',
		);

		this.observe(
			this.#workspaceContext.documentRootAccess,
			(value) => (this._documentRootAccess = value),
			'_observeDocumentRootAccess',
		);

		this.observe(
			this.#workspaceContext.documentStartNode,
			(value) => (this._documentStartNode = value),
			'_observeDocumentStartNode',
		);

		this.observe(
			this.#workspaceContext.mediaRootAccess,
			(value) => (this._mediaRootAccess = value),
			'_observeMediaRootAccess',
		);

		this.observe(
			this.#workspaceContext.mediaStartNode,
			(value) => (this._mediaStartNode = value),
			'_observeMediaStartNode',
		);
	}

	#onSectionsChange(event: UmbChangeEvent) {
		event.stopPropagation();
		const target = event.target as UmbInputSectionElement;
		// TODO make contexts method
		this.#workspaceContext?.updateProperty('sections', target.selection);
	}

	#onAllowAllLanguagesChange(event: UUIBooleanInputEvent) {
		event.stopPropagation();
		const target = event.target;
		// TODO make contexts method
		this.#workspaceContext?.updateProperty('hasAccessToAllLanguages', target.checked);
	}

	#onLanguagePermissionChange(event: UmbChangeEvent) {
		event.stopPropagation();
		const target = event.target as UmbInputLanguageElement;
		// TODO make contexts method
		this.#workspaceContext?.updateProperty('languages', target.selection);
	}

	#onAllowAllDocumentsChange(event: UUIBooleanInputEvent) {
		event.stopPropagation();
		const target = event.target;
		// TODO make contexts method
		this.#workspaceContext?.updateProperty('documentRootAccess', target.checked);
		this.#workspaceContext?.updateProperty('documentStartNode', null);
	}

	#onDocumentStartNodeChange(event: CustomEvent) {
		event.stopPropagation();
		const target = event.target as UmbInputDocumentElement;
		const selected = target.selection?.[0];
		// TODO make contexts method
		this.#workspaceContext?.updateProperty('documentStartNode', selected ? { unique: selected } : null);
	}

	#onAllowAllMediaChange(event: UUIBooleanInputEvent) {
		event.stopPropagation();
		const target = event.target;
		// TODO make contexts method
		this.#workspaceContext?.updateProperty('mediaRootAccess', target.checked);
		this.#workspaceContext?.updateProperty('mediaStartNode', null);
	}

	#onMediaStartNodeChange(event: CustomEvent) {
		event.stopPropagation();
		const target = event.target as UmbInputMediaElement;
		const selected = target.selection?.[0];
		// TODO make contexts method
		this.#workspaceContext?.updateProperty('mediaStartNode', selected ? { unique: selected } : null);
	}

	override render() {
		return html`
			<umb-entity-detail-workspace-editor class="uui-text" back-path=${UMB_USER_GROUP_ROOT_WORKSPACE_PATH}>
				${this.#renderHeader()} ${this.#renderMain()}
			</umb-entity-detail-workspace-editor>
		`;
	}

	async #onIconClick() {
		const [alias, color] = this._icon?.replace('color-', '')?.split(' ') ?? [];
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_ICON_PICKER_MODAL, {
			value: {
				icon: alias,
				color: color,
			},
		});

		modalContext?.onSubmit().then((saved) => {
			if (saved.icon && saved.color) {
				this.#workspaceContext?.updateProperty('icon', `${saved.icon} color-${saved.color}`);
			} else if (saved.icon) {
				this.#workspaceContext?.updateProperty('icon', saved.icon);
			}
		});
	}

	#onNameAndAliasChange(event: InputEvent & { target: UmbInputWithAliasElement }) {
		this.#workspaceContext?.updateProperty('name', event.target.value ?? '');
		this.#workspaceContext?.updateProperty('alias', event.target.alias ?? '');
	}

	#renderHeader() {
		return html`
			<div id="header" slot="header">
				<uui-button id="icon" compact label="icon" look="outline" @click=${this.#onIconClick}>
					<umb-icon name=${this._icon || ''}></umb-icon>
				</uui-button>

				<umb-input-with-alias
					id="name"
					label=${this.localize.term('placeholders_entername')}
					.value=${this._name}
					alias=${ifDefined(this._alias)}
					?auto-generate-alias=${this._isNew}
					?alias-readonly=${this._aliasCanBeChanged === false}
					@change=${this.#onNameAndAliasChange}
					${umbFocus()}>
				</umb-input-with-alias>
			</div>
		`;
	}

	#renderMain() {
		if (!this._unique) return nothing;

		return html`
			<div id="main">
				<umb-stack>
					<uui-box>
						<div slot="headline"><umb-localize key="user_assignAccess"></umb-localize></div>

						<umb-property-layout
							label=${this.localize.term('main_sections')}
							description=${this.localize.term('user_sectionsHelp')}>
							<umb-input-section
								slot="editor"
								.selection=${this._sections}
								@change=${this.#onSectionsChange}></umb-input-section>
						</umb-property-layout>

						${this.#renderLanguageAccess()} ${this.#renderDocumentAccess()} ${this.#renderMediaAccess()}
					</uui-box>

					<uui-box>
						<div slot="headline"><umb-localize key="user_permissionsDefault"></umb-localize></div>

						<umb-property-layout label="Permissions"
							><umb-extension-slot
								slot="editor"
								type="userPermission"
								default-element="umb-input-user-permission"></umb-extension-slot
						></umb-property-layout>

						<umb-property-layout label="Entity permissions" description="Assign permissions for specific entities">
							<umb-user-group-entity-user-permission-list slot="editor"></umb-user-group-entity-user-permission-list>
						</umb-property-layout>
					</uui-box>

					<uui-box>
						<div slot="headline"><umb-localize key="user_permissionsGranular"></umb-localize></div>
						<umb-user-group-granular-permission-list></umb-user-group-granular-permission-list>
					</uui-box>
				</umb-stack>
			</div>
		`;
	}

	#renderLanguageAccess() {
		return html`
			<umb-property-layout
				label=${this.localize.term('treeHeaders_languages')}
				description=${this.localize.term('user_languagesHelp')}>
				<div slot="editor">
					<uui-toggle
						style="margin-bottom: var(--uui-size-space-3);"
						label="${this.localize.term('user_allowAccessToAllLanguages')}"
						.checked=${this._hasAccessToAllLanguages}
						@change=${this.#onAllowAllLanguagesChange}></uui-toggle>

					${this._hasAccessToAllLanguages === false
						? html`
								<umb-input-language
									.selection=${this._languages}
									@change=${this.#onLanguagePermissionChange}></umb-input-language>
							`
						: nothing}
				</div>
			</umb-property-layout>
		`;
	}

	#renderDocumentAccess() {
		return html`
			<umb-property-layout
				label=${this.localize.term('defaultdialogs_selectContentStartNode')}
				description=${this.localize.term('user_startnodehelp')}>
				<div slot="editor">
					<uui-toggle
						style="margin-bottom: var(--uui-size-space-3);"
						label="${this.localize.term('user_allowAccessToAllDocuments')}"
						.checked=${this._documentRootAccess}
						@change=${this.#onAllowAllDocumentsChange}></uui-toggle>
				</div>

				${this._documentRootAccess === false
					? html`
							<umb-input-document
								slot="editor"
								max="1"
								.selection=${this._documentStartNode?.unique ? [this._documentStartNode.unique] : []}
								@change=${this.#onDocumentStartNodeChange}></umb-input-document>
						`
					: nothing}
			</umb-property-layout>
		`;
	}

	#renderMediaAccess() {
		return html`
			<umb-property-layout
				label=${this.localize.term('defaultdialogs_selectMediaStartNode')}
				description=${this.localize.term('user_mediastartnodehelp')}>
				<div slot="editor">
					<uui-toggle
						style="margin-bottom: var(--uui-size-space-3);"
						label="${this.localize.term('user_allowAccessToAllMedia')}"
						.checked=${this._mediaRootAccess}
						@change=${this.#onAllowAllMediaChange}></uui-toggle>
				</div>

				${this._mediaRootAccess === false
					? html`
							<umb-input-media
								slot="editor"
								max="1"
								.selection=${this._mediaStartNode?.unique ? [this._mediaStartNode.unique] : []}
								@change=${this.#onMediaStartNodeChange}></umb-input-media>
						`
					: nothing}
			</umb-property-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				height: 100%;
			}

			#header {
				display: flex;
				flex: 1 1 auto;
				gap: var(--uui-size-space-2);
				align-items: center;
			}

			#icon {
				font-size: var(--uui-size-5);
				height: 30px;
				width: 30px;
			}

			#name {
				width: 100%;
				flex: 1 1 auto;
				align-items: center;
			}

			#main {
				padding: var(--uui-size-layout-1);
			}

			uui-input {
				width: 100%;
			}
		`,
	];
}

export default UmbUserGroupWorkspaceEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-workspace-editor': UmbUserGroupWorkspaceEditorElement;
	}
}
