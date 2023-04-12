import { UmbVariantId } from '../../../variants/variant-id.class';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { ArrayState } from '@umbraco-cms/backoffice/observable-api';

export type ActiveVariant = {
	index: number;
	culture: string | null;
	segment: string | null;
};

/**
 * @export
 * @class UmbWorkspaceSplitViewManager
 * @description - Class managing the split view state for a workspace context.
 */
export class UmbWorkspaceSplitViewManager {
	#host: UmbControllerHostElement;

	#activeVariantsInfo = new ArrayState<ActiveVariant>([], (x) => x.index);
	public readonly activeVariantsInfo = this.#activeVariantsInfo.asObservable();

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	private _routeBase?: string;
	public getWorkspaceRoute(): string | undefined {
		return this._routeBase;
	}
	public setWorkspaceRoute(route: string | undefined) {
		this._routeBase = route;
	}

	setActiveVariant(index: number, culture: string | null, segment: string | null) {
		this.#activeVariantsInfo.appendOne({ index, culture, segment });
	}

	getActiveVariants() {
		return this.#activeVariantsInfo.getValue();
	}

	public removeActiveVariant(index: number) {
		if (this.getActiveVariants().length > 1) {
			this.#activeVariantsInfo.removeOne(index);
		}
	}

	public activeVariantByIndex(index: number) {
		return this.#activeVariantsInfo.getObservablePart((data) => data[index] || undefined);
	}

	public switchVariant(index: number, variantId: UmbVariantId) {
		// TODO: remember current path and extend url with it.
		// TODO: construct URl with all active routes:
		// TODO: use method for generating variant url:
		const workspaceRoute = this.getWorkspaceRoute();
		if (workspaceRoute) {
			const activeVariants = this.getActiveVariants();
			if (activeVariants && index < activeVariants.length) {
				const newVariants = [...activeVariants];
				newVariants[index] = { index, culture: variantId.culture, segment: variantId.segment };

				const variantPart: string = newVariants.map((v) => new UmbVariantId(v).toString()).join('_&_');

				history.pushState(null, '', `${workspaceRoute}/${variantPart}`);
				return true;
			}
		}
		return false;
	}

	public openSplitView(newVariant: UmbVariantId) {
		// TODO: remember current path and extend url with it.
		// TODO: construct URl with all active routes:
		// TODO: use method for generating variant url:

		const currentVariant = this.getActiveVariants()[0];
		const workspaceRoute = this.getWorkspaceRoute();
		if (currentVariant && workspaceRoute) {
			history.pushState(null, '', `${workspaceRoute}/${new UmbVariantId(currentVariant)}_&_${newVariant.toString()}`);
			return true;
		}
		return false;
	}

	public closeSplitView(index: number) {
		const workspaceRoute = this.getWorkspaceRoute();
		if (workspaceRoute) {
			const activeVariants = this.getActiveVariants();
			if (activeVariants && index < activeVariants.length) {
				const newVariants = activeVariants.filter((x) => x.index !== index);

				const variantPart: string = newVariants.map((v) => new UmbVariantId(v).toString()).join('_&_');

				history.pushState(null, '', `${workspaceRoute}/${variantPart}`);
				return true;
			}
		}
		return false;
	}
}
