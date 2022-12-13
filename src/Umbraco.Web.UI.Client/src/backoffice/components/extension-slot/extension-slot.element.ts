import { html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { map } from 'rxjs';
import { repeat } from 'lit/directives/repeat.js';
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

    @state()
    private _extensions:{alias: string, weight: number, component: HTMLElement|null}[] = [];

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
		this.observe(
			umbExtensionsRegistry
				?.extensionsOfType(this.type)
				.pipe(map((extensions) => extensions.filter(this.filter))),
			async (extensions: ManifestTypes[]) => {

                const oldLength = this._extensions.length;
                this._extensions = this._extensions.filter(current => extensions.find(incoming => incoming.alias === current.alias));
                if(this._extensions.length !== oldLength) {
                    this.requestUpdate('_extensions');
                }

                extensions.forEach(async (extension: ManifestTypes) => {

                    const hasExt = this._extensions.find(x => x.alias === extension.alias);
                    if(!hasExt) {
                        const extensionObject = {alias: extension.alias, weight: (extension as any).weight || 0, component: null};
                        this._extensions.push(extensionObject);
                        const component = await createExtensionElement(extension);
                        if(component) {
                            // TODO: Consider if this is the best way to parse meta data to the component it self?
                            if((extension as any)?.meta) {
                                (component as any).extensionMeta = (extension as any).meta;
                            }
                            extensionObject.component = component;

                            // sort:
                            // TODO: Make sure its right to have highest last?
                            this._extensions.sort((a, b) => a.weight - b.weight);
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

    render() {
        // TODO: check if we can use repeat directly.
        return repeat(this._extensions, (ext) => ext.alias, (ext, index) => ext.component);
    }
}

declare global {
    interface HTMLElementTagNameMap {
        'umb-extension-slot': UmbExtensionSlotElement;
    }
}