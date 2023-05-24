/* eslint-disable @typescript-eslint/no-explicit-any */
import {
	css,
	nothing,
	repeat,
	TemplateResult,
	customElement,
	property,
	state,
} from '@umbraco-cms/backoffice/external/lit';
import { map } from '@umbraco-cms/backoffice/external/rxjs';
import { createExtensionElement, isManifestElementableType } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

export type InitializedExtension = { alias: string; weight: number; component: HTMLElement | null };

/**
 * @element umb-extension-slot
 * @description
 * @slot default - slot for inserting additional things into this slot.
 * @export
 * @class UmbExtensionSlot
 * @extends {UmbLitElement}
 */
@customElement('umb-extension-slot')
export class UmbExtensionSlotElement extends UmbLitElement {
	@state()
	private _extensions: InitializedExtension[] = [];

	@property({ type: String })
	public type = '';

	@property({ type: Object, attribute: false })
	public filter: (manifest: any) => boolean = () => true;

	private _props?: Record<string, any> = {};
	@property({ type: Object, attribute: false })
	get props() {
		return this._props;
	}
	set props(newVal) {
		this._props = newVal;
		// TODO: we could optimize this so we only re-set the updated props.
		this.#assignPropsToAllComponents();
	}

	@property({ type: String, attribute: 'default-element' })
	public defaultElement = '';

	@property()
	public renderMethod: (extension: InitializedExtension) => TemplateResult<1 | 2> | HTMLElement | null = (extension) =>
		extension.component;

	connectedCallback(): void {
		super.connectedCallback();
		this._observeExtensions();
	}

	private _observeExtensions() {
		this.observe(
			umbExtensionsRegistry?.extensionsOfType(this.type).pipe(map((extensions) => extensions.filter(this.filter))),
			async (extensions) => {
				const oldLength = this._extensions.length;
				this._extensions = this._extensions.filter((current) =>
					extensions.find((incoming) => incoming.alias === current.alias)
				);
				if (this._extensions.length !== oldLength) {
					this.requestUpdate('_extensions');
				}

				extensions.forEach(async (extension) => {
					const hasExt = this._extensions.find((x) => x.alias === extension.alias);
					if (!hasExt) {
						const extensionObject: InitializedExtension = {
							alias: extension.alias,
							weight: (extension as any).weight || 0,
							component: null,
						};
						this._extensions.push(extensionObject);
						let component;

						if (isManifestElementableType(extension)) {
							component = await createExtensionElement(extension);
						} else if (this.defaultElement) {
							component = document.createElement(this.defaultElement);
						} else {
							// TODO: Lets make an console.error in this case?
						}
						if (component) {
							this.#assignProps(component);
							(component as any).manifest = extension;
							extensionObject.component = component;

							// sort:
							// TODO: Make sure its right to have highest last?
							this._extensions.sort((a, b) => b.weight - a.weight);
						} else {
							// Remove cause we could not get the component, so we will get rid of this.
							//this._extensions.splice(this._extensions.indexOf(extensionObject), 1);
							// Actually not, because if, then the same extension would come around again in next update.
						}
						this.requestUpdate('_extensions');
					}
				});
			}
		);
	}

	#assignPropsToAllComponents() {
		this._extensions.forEach((ext) => this.#assignProps(ext.component));
	}

	#assignProps = (component: HTMLElement | null) => {
		if (!component || !this._props) return;

		Object.keys(this._props).forEach((key) => {
			(component as any)[key] = this._props?.[key];
		});
	};

	render() {
		// TODO: check if we can use repeat directly.
		return repeat(
			this._extensions,
			(ext) => ext.alias,
			(ext) => this.renderMethod(ext) || nothing
		);
	}

	static styles = css`
		:host {
			display: contents;
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-extension-slot': UmbExtensionSlotElement;
	}
}
