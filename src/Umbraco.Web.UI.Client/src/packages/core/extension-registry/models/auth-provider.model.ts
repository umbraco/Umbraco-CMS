import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';
import type { UUIInterfaceColor, UUIInterfaceLook } from '@umbraco-cms/backoffice/external/uui';

/**
 * Represents an authentication provider that can be used to authenticate users.
 * The provider needs to be registered in the API that the authorization request is sent to in order to be used.
 *
 * @see {forProviderName} for the provider name that this provider is for.
 */
export interface ManifestAuthProvider extends ManifestElement {
	type: 'authProvider';

	/**
	 * The provider name that this provider is for.
	 * @examples 'Umbraco.Github'
	 */
	forProviderName: string;

	meta?: MetaAuthProvider;
}

export interface MetaAuthProvider {
	/**
	 * The label of the provider that is shown to the user.
	 */
	label?: string;

	/**
	 * The default view of the provider that is shown to the user.
	 * If no element is provided, then the button will be rendered as a @see {UUIButtonElement} using these options.
	 */
	defaultView: {
		/**
		 * The icon of the provider that is shown to the user.
		 * @examples ['icon-cloud', 'icon-github', 'icon-google', 'icon-facebook', 'icon-twitter', 'icon-x', 'icon-microsoft']
		 * @default 'icon-cloud'
		 */
		icon?: string;

		/**
		 * The color of the provider that is shown to the user.
		 * @default 'secondary'
		 */
		color?: UUIInterfaceLook;

		/**
		 * The look of the provider that is shown to the user.
		 * @default 'default'
		 */
		look?: UUIInterfaceColor;
	};

	/**
	 * If true, the Umbraco backoffice login will be disabled.
	 * @default false
	 */
	denyLocalLogin?: boolean;

	/**
	 * If true, the user will be redirected to the provider's login page immediately.
	 * @default false
	 */
	autoRedirect?: boolean;
}
