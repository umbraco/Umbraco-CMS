import type { UmbAction } from '@umbraco-cms/backoffice/action';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

export interface UmbCurrentUserActionArgs<MetaArgsType> {
	meta: MetaArgsType;
}

export interface UmbCurrentUserAction<ArgsMetaType = never> extends UmbAction<UmbCurrentUserActionArgs<ArgsMetaType>> {
	/**
	 * The href location, the action will act as a link.
	 * @returns {Promise<string | undefined>}
	 */
	getHref(): Promise<string | undefined>;

	/**
	 * The `execute` method, the action will act as a button.
	 * @returns {Promise<void>}
	 */
	execute(): Promise<void>;
}

export interface ManifestCurrentUserAction<MetaType extends MetaCurrentUserAction = MetaCurrentUserAction>
	extends ManifestElementAndApi<UmbControllerHostElement, UmbCurrentUserAction<MetaType>>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'currentUserAction';
	meta: MetaType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaCurrentUserAction {}

export interface ManifestCurrentUserActionDefaultKind<
	MetaType extends MetaCurrentUserActionDefaultKind = MetaCurrentUserActionDefaultKind,
> extends ManifestCurrentUserAction<MetaType> {
	type: 'currentUserAction';
	kind: 'default';
}

export interface MetaCurrentUserActionDefaultKind extends MetaCurrentUserAction {
	/**
	 * An icon to represent the action to be performed
	 * @examples [
	 *   "icon-box",
	 *   "icon-grid"
	 * ]
	 */
	icon?: string;

	/**
	 * The friendly name of the action to perform
	 * @examples [
	 *   "Create",
	 *   "Create Content Template"
	 * ]
	 */
	label: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbCurrentUserAction: ManifestCurrentUserAction;
	}
}
