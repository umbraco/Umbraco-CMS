import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../../core/context';
import { createExtensionElement, UmbExtensionRegistry } from '../../../../core/extension';
import { map } from 'rxjs';
import { ManifestEditor } from '../../../../core/models';

@customElement('umb-editor-entity')
export class UmbEditorEntityElement extends UmbContextConsumerMixin(LitElement) {
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
		this._useEditors();
	}

	@state()
	private _element?: any;

	private _extensionRegistry?: UmbExtensionRegistry;

	constructor() {
		super();

		this.consumeContext('umbExtensionRegistry', (extensionRegistry: UmbExtensionRegistry) => {
			this._extensionRegistry = extensionRegistry;
			this._useEditors();
		});
	}

	private _useEditors() {
		if (!this._extensionRegistry) return;

		this._extensionRegistry
			.extensionsOfType('editor')
			.pipe(map((editors) => editors.find((editor) => editor.meta.entityType === this.entityType)))
			.subscribe((editor) => {
				this._createElement(editor);
			});
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
