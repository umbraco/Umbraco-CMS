import { UMB_PROPERTY_TYPE_CONTEXT } from './content-type-design-editor-property.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';

export class UmbPropertyTypeContext extends UmbContextBase<UmbPropertyTypeContext> {
	#alias = new UmbStringState(undefined);
	public readonly alias = this.#alias.asObservable();
	#label = new UmbStringState(undefined);
	public readonly label = this.#label.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_PROPERTY_TYPE_CONTEXT);
	}

	public setAlias(alias: string | undefined): void {
		this.#alias.setValue(alias);
	}
	public getAlias(): string | undefined {
		return this.#alias.getValue();
	}
	public setLabel(label: string | undefined): void {
		this.#label.setValue(label);
	}
	public getLabel(): string | undefined {
		return this.#label.getValue();
	}

	public override destroy(): void {
		super.destroy();
		this.#alias.destroy();
		this.#label.destroy();
	}
}
