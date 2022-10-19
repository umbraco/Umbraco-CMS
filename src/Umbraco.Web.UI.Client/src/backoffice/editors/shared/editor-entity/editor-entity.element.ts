import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { map } from 'rxjs';

import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { createExtensionElement } from '@umbraco-cms/extensions-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { ManifestEditor } from '@umbraco-cms/models';

@customElement('umb-editor-entity')
export class UmbEditorEntityElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];

	@property()
	public entityKey!: string;

	private _entityType = '';
	@property()
	public get entityType(): string {
		return this._entityType;
	}
	public set entityType(value: string) {
		this._entityType = value;
		this._observeEditors();
	}

	@state()
	private _element?: any;

	private _currentEditorAlias = '';

	connectedCallback(): void {
		super.connectedCallback();
		this._observeEditors();
	}

	private _observeEditors() {
		this.observe<ManifestEditor>(
			umbExtensionsRegistry
				.extensionsOfType('editor')
				.pipe(map((editors) => editors.find((editor) => editor.meta.entityType === this.entityType))),
			(editor) => {
				// don't rerender editor if it's the same
				if (this._currentEditorAlias === editor.alias) return;
				this._currentEditorAlias = editor.alias;
				this._createElement(editor);
			}
		);
	}

	private async _createElement(editor?: ManifestEditor) {
		// TODO: implement fallback editor
		const fallbackEditor = document.createElement('div');
		fallbackEditor.innerHTML = '<p>No editor found</p>';

		if (!editor) {
			this._element = fallbackEditor;
			return;
		}

		try {
			this._element = (await createExtensionElement(editor)) as any;
			this._element.entityKey = this.entityKey;
		} catch (error) {
			this._element = fallbackEditor;
		}
	}

	render() {
		return html`${this._element}`;
	}
}

export default UmbEditorEntityElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-entity': UmbEditorEntityElement;
	}
}
