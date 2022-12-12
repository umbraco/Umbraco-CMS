import { html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { map } from 'rxjs';
import { ManifestTypes, umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { createExtensionElement } from '@umbraco-cms/extensions-api';

/**
 * @element umb-extension-slot
 * @description
 * @slot default - slot for inserting additional things into this slot.
 * @export
 * @class UmbExtensionSlot
 * @extends {UmbObserverMixin(LitElement)}
 */
@customElement('umb-extension-slot')
export class UmbExtensionSlotElement extends UmbObserverMixin(LitElement) {


    private _extensions = new Map<ManifestTypes, HTMLElement>()

    @property({ type: String })
    public type= "";

	@property({ type: Object, attribute: false })
    public filter: (manifest:ManifestTypes) => boolean = () => true;

    constructor() {
        super();


		/*
		this.extensionManager = new ExtensionManager(this, (x) => {x.meta.entityType === this.entityType}, (extensionManifests) => {
			this._createElement(extensionManifests[0]);
		});
		*/
	}

    connectedCallback(): void {
        super.connectedCallback();
		this._observeExtensions();
    }

    private _observeExtensions() {

        console.log("_observeExtensions", this.type, this.filter)
		this.observe(
			umbExtensionsRegistry
				?.extensionsOfType(this.type)
				.pipe(map((extensions) => extensions.filter(this.filter))),
			async (extensions: ManifestTypes[]) => {

                extensions.forEach(async (extension: ManifestTypes) => {
                    const component = await createExtensionElement(extension);
                    if(component) {
                        this._extensions.set(extension, component);
                    } else {
                        this._extensions.delete(extension);
                    }
                });

			}
		);
	}

    render() {

        const elements = [];
        for (const value of this._extensions.values()) {
            elements.push(value);
        }
        return html`${elements}`;
    }
}

declare global {
    interface HTMLElementTagNameMap {
        'umb-extension-slot': UmbExtensionSlotElement;
    }
}