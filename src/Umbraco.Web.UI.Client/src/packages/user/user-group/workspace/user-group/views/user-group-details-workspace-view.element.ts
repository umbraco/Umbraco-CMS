import type { UmbUserGroupDetailModel } from '../../../types.js';
import { UMB_USER_GROUP_WORKSPACE_CONTEXT } from '../user-group-workspace.context-token.js';
import { css, customElement, html, nothing, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbInputEntityDataElement } from '@umbraco-cms/backoffice/entity-data-picker';
import type { UmbInputLanguageElement } from '@umbraco-cms/backoffice/language';
import type { UmbInputSectionElement } from '@umbraco-cms/backoffice/section';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import type { UUIBooleanInputEvent } from '@umbraco-cms/backoffice/external/uui';

import '../components/user-group-entity-type-permission-groups.element.js';
import '@umbraco-cms/backoffice/entity-data-picker';

@customElement('umb-user-group-details-workspace-view')
export class UmbUserGroupDetailsWorkspaceViewElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	private _unique?: UmbUserGroupDetailModel['unique'];

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
	private _elementStartNode?: UmbUserGroupDetailModel['elementStartNode'];

	@state()
	private _elementRootAccess: UmbUserGroupDetailModel['elementRootAccess'] = false;

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
		this.observe(this.#workspaceContext?.unique, (value) => (this._unique = value ?? undefined), '_observeUnique');
		this.observe(this.#workspaceContext?.sections, (value) => (this._sections = value ?? []), '_observeSections');
		this.observe(this.#workspaceContext?.languages, (value) => (this._languages = value ?? []), '_observeLanguages');
		this.observe(
			this.#workspaceContext?.hasAccessToAllLanguages,
			(value) => (this._hasAccessToAllLanguages = value ?? false),
			'_observeHasAccessToAllLanguages',
		);

		this.observe(
			this.#workspaceContext?.documentRootAccess,
			(value) => (this._documentRootAccess = value ?? false),
			'_observeDocumentRootAccess',
		);

		this.observe(
			this.#workspaceContext?.documentStartNode,
			(value) => (this._documentStartNode = value),
			'_observeDocumentStartNode',
		);

		this.observe(
			this.#workspaceContext?.elementRootAccess,
			(value) => (this._elementRootAccess = value ?? false),
			'_observeElementRootAccess',
		);

		this.observe(
			this.#workspaceContext?.elementStartNode,
			(value) => (this._elementStartNode = value),
			'_observeElementStartNode',
		);

		this.observe(
			this.#workspaceContext?.mediaRootAccess,
			(value) => (this._mediaRootAccess = value ?? false),
			'_observeMediaRootAccess',
		);

		this.observe(
			this.#workspaceContext?.mediaStartNode,
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
		// TODO: get back to this when documents have been decoupled from users.
		// The event target is deliberately set to any to avoid an import cycle with documents.
		const target = event.target as any;
		const selected = target.selection?.[0];
		// TODO make contexts method
		this.#workspaceContext?.updateProperty('documentStartNode', selected ? { unique: selected } : null);
	}

	#onAllowAllElementsChange(event: UUIBooleanInputEvent) {
		event.stopPropagation();
		const target = event.target;
		// TODO make contexts method
		this.#workspaceContext?.updateProperty('elementRootAccess', target.checked);
		this.#workspaceContext?.updateProperty('elementStartNode', null);
	}

	#onElementStartNodeChange(event: CustomEvent & { target: UmbInputEntityDataElement }) {
		event.stopPropagation();
		// TODO: get back to this when elements have been decoupled from users.
		const target = event.target;
		const selected = target.selection?.[0];
		// TODO make contexts method
		this.#workspaceContext?.updateProperty('elementStartNode', selected ? { unique: selected } : null);
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
		// TODO: get back to this when media have been decoupled from users.
		// The event target is deliberately set to any to avoid an import cycle with media.
		const target = event.target as any;
		const selected = target.selection?.[0];
		// TODO make contexts method
		this.#workspaceContext?.updateProperty('mediaStartNode', selected ? { unique: selected } : null);
	}

	override render() {
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
						${this.#renderElementAccess()}
					</uui-box>

					${this.#renderPermissionGroups()}
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
						label=${this.localize.term('user_allowAccessToAllLanguages')}
						.checked=${this._hasAccessToAllLanguages}
						@change=${this.#onAllowAllLanguagesChange}></uui-toggle>

					${when(
						this._hasAccessToAllLanguages === false,
						() => html`
							<umb-input-language
								.selection=${this._languages}
								@change=${this.#onLanguagePermissionChange}></umb-input-language>
						`,
					)}
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
						label=${this.localize.term('user_allowAccessToAllDocuments')}
						.checked=${this._documentRootAccess}
						@change=${this.#onAllowAllDocumentsChange}></uui-toggle>
				</div>
				${when(
					this._documentRootAccess === false,
					() => html`
						<umb-input-document
							slot="editor"
							max="1"
							.selection=${this._documentStartNode?.unique ? [this._documentStartNode.unique] : []}
							@change=${this.#onDocumentStartNodeChange}>
						</umb-input-document>
					`,
				)}
			</umb-property-layout>
		`;
	}

	#renderElementAccess() {
		return html`
			<umb-property-layout
				label=${this.localize.term('user_selectElementStartNode')}
				description=${this.localize.term('user_selectElementStartNodeDescription')}>
				<div slot="editor">
					<uui-toggle
						style="margin-bottom: var(--uui-size-space-3);"
						label=${this.localize.term('user_allowAccessToAllElements')}
						.checked=${this._elementRootAccess}
						@change=${this.#onAllowAllElementsChange}></uui-toggle>
				</div>
				${when(
					this._elementRootAccess === false,
					() => html`
						<umb-input-entity-data
							slot="editor"
							max="1"
							.selection=${this._elementStartNode?.unique ? [this._elementStartNode.unique] : []}
							.dataSourceAlias=${'Umb.PropertyEditorDataSource.ElementFolder'}
							.dataSourceConfig=${[]}
							@change=${this.#onElementStartNodeChange}>
						</umb-input-entity-data>
					`,
				)}
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
						label=${this.localize.term('user_allowAccessToAllMedia')}
						.checked=${this._mediaRootAccess}
						@change=${this.#onAllowAllMediaChange}></uui-toggle>
				</div>
				${when(
					this._mediaRootAccess === false,
					() => html`
						<umb-input-media
							slot="editor"
							max="1"
							.selection=${this._mediaStartNode?.unique ? [this._mediaStartNode.unique] : []}
							@change=${this.#onMediaStartNodeChange}>
						</umb-input-media>
					`,
				)}
			</umb-property-layout>
		`;
	}

	#renderPermissionGroups() {
		return html`<umb-user-group-entity-type-permission-groups></umb-user-group-entity-type-permission-groups>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				height: 100%;
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

export { UmbUserGroupDetailsWorkspaceViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-group-details-workspace-view': UmbUserGroupDetailsWorkspaceViewElement;
	}
}
