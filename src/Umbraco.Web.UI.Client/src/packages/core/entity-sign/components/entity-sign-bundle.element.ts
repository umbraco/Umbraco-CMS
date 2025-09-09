import { customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '../../lit-element/lit-element.element';
import type { ManifestEntitySign } from '../types';
import {
	UmbExtensionElementAndApiInitializer,
	UmbExtensionsElementAndApiInitializer,
} from '@umbraco-cms/backoffice/extension-api';
import type { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-entity-sign-bundle')
export class UmbEntitySignBundleElement extends UmbLitElement {
	@property({ type: String, attribute: 'entity-type', reflect: false })
	get entityType(): string | undefined {
		return this._entityType;
	}
	set entityType(value: string | undefined) {
		this._entityType = value ?? undefined;
		this.#gotEntityType();
	}

	@state()
	_entityType?: string;

	@state()
	_signs?: Array<UmbExtensionElementAndApiInitializer<ManifestEntitySign>>;

	@state()
	_labels: Map<string, string> = new Map();

	#signLabelObservations: Array<UmbObserverController<string>> = [];

	#manifestFilter = (manifest: ManifestEntitySign) => {
		// If entity-type check, then ensure it matches:
		if (manifest.forEntityTypes && !manifest.forEntityTypes.includes(this._entityType!)) {
			return false;
		}

		// TODO: Implement forEntityFlags check

		return true;
	};

	#gotEntityType() {
		if (!this._entityType) {
			this.removeUmbControllerByAlias('extensionsInitializer');
			return;
		}

		new UmbExtensionsElementAndApiInitializer(
			this,
			umbExtensionsRegistry,
			'entitySign',
			(manifest: ManifestEntitySign) => [{ meta: manifest.meta }],
			this.#manifestFilter,
			(signs) => {
				// Clean up old observers
				this.#signLabelObservations.forEach((observation) => this.removeUmbController(observation));
				this.#signLabelObservations = [];
				// Setup observers for the labels
				signs.forEach((sign) => {
					if (sign.api?.label) {
						const labelObservation = this.observe(sign.api.label, (label) => {
							this._labels.set(sign.alias, label);
							this.requestUpdate('_labels');
						});
						this.#signLabelObservations.push(labelObservation);
					}
				});
				this._signs = signs;
			},
			'extensionsInitializer',
		);
	}

	override render() {
		return this._signs
			? repeat(
					this._signs,
					(c) => c.alias,
					(c) => c.component,
				)
			: nothing;
	}
}
