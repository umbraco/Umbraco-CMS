/* eslint-disable @typescript-eslint/no-explicit-any */
import { css, repeat, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { map } from '@umbraco-cms/backoffice/external/rxjs';
import { ManifestBase, UmbElementExtensionController } from '@umbraco-cms/backoffice/extension-api';
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

// TODO: Fire change event.
// TODO: Make property that reveals the amount of displayed/permitted extensions.
@customElement('umb-extension-slot')
export class UmbExtensionSlotElement extends UmbLitElement {
	@state()
	private _extensions: Array<UmbElementExtensionController> = [];

	@state()
	private _permittedExts: Array<UmbElementExtensionController> = [];

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
		this._extensions.forEach((ext) => (ext.properties = this._props));
	}

	@property({ type: String, attribute: 'default-element' })
	public defaultElement = '';

	connectedCallback(): void {
		super.connectedCallback();
		this._observeExtensions();
	}

	private _observeExtensions() {
		// TODO: This could be optimized by just getting the aliases, well depends on the filter (revisit one day to see how much filter is used)
		this.observe(
			umbExtensionsRegistry?.extensionsOfType(this.type).pipe(map((extensions) => extensions.filter(this.filter))),
			this.#gotManifests,
			'_observeExtensions'
		);
	}

	#gotManifests = (manifests: Array<ManifestBase>) => {
		const oldValue = this._extensions;
		const oldLength = this._extensions.length;

		// Clean up extensions that are no longer.
		this._extensions = this._extensions.filter((controller) => {
			if (!manifests.find((manifest) => manifest.alias === controller.alias)) {
				controller.destroy();
				// destroying the controller will, if permitted, make a last callback with isPermitted = false. And thereby call a requestUpdate.
				return false;
			}
			return true;
		});

		// ---------------------------------------------------------------
		// May change this into a Extensions Manager Controller???
		// ---------------------------------------------------------------

		manifests.forEach((manifest) => {
			const existing = this._extensions.find((x) => x.alias === manifest.alias);
			if (!existing) {
				const controller = new UmbElementExtensionController(
					this,
					umbExtensionsRegistry,
					manifest.alias,
					this.#extensionChanged,
					this.defaultElement
				);
				controller.properties = this._props;
				this._extensions.push(controller);
			}
		});
	};

	#extensionChanged = (isPermitted: boolean, controller: UmbElementExtensionController) => {
		const oldValue = this._permittedExts;
		const oldLength = oldValue.length;
		const existingIndex = this._permittedExts.indexOf(controller);
		if (isPermitted) {
			if (existingIndex === -1) {
				this._permittedExts.push(controller);
			}
		} else {
			if (existingIndex !== -1) {
				this._permittedExts.splice(existingIndex, 1);
			}
		}
		if (oldLength !== this._permittedExts.length) {
			this._permittedExts.sort((a, b) => b.weight - a.weight);
			this.requestUpdate('_permittedExts', oldValue);
		}
	};

	render() {
		// TODO: check if we can use repeat directly.
		return repeat(
			this._permittedExts,
			(ext) => ext.alias,
			(ext) => ext.component
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
