import { Link } from '../../externals.js';
import { isLocalLinkHref } from './link-attributes.function.js';

export const UmbLink = Link.extend({
	name: 'umbLink',

	addAttributes() {
		return {
			...this.parent?.(),
			'data-anchor': { default: null },
			'data-culture': { default: null },
			target: { default: null },
			title: { default: null },
			// `type` carries the entity type of a local link, which the server needs to resolve it to a URL and
			// strips from the rendered markup afterwards. On any other href nothing strips it and it reaches the
			// front end, where `<a type>` is invalid unless it holds a MIME type (#23648) — so only serialize it
			// onto a local link.
			type: {
				default: null,
				renderHTML: (attributes) =>
					attributes.type && isLocalLinkHref(attributes.href) ? { type: attributes.type } : {},
			},
		};
	},

	// TODO: [LK] Review why `addOptions()` is not typed correctly here.
	// ref: https://github.com/ueberdosis/tiptap/issues/6670
	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
	// @ts-expect-error
	addOptions() {
		return {
			...this.parent?.(),
			// Empty (not omitted): the parent Link extension's own addOptions() defaults `HTMLAttributes` to
			// `{ target: '_blank', rel: 'noopener noreferrer nofollow', class: null }`, which addAttributes()
			// then uses as the schema-level default for each link's own `rel`/`target`/`class` attrs. Overriding
			// with `{}` here keeps those attributes unset by default, same as before `data-router-slot` moved
			// to the MarkView below.
			HTMLAttributes: {},
		};
	},

	// Renders the live anchor by hand so `data-router-slot="disabled"` (needed to stop the backoffice's
	// SPA router intercepting clicks on RTE-authored links) stays a live-DOM-only concern and never leaks
	// into the serialized/stored HTML via `renderHTML` (#22654).
	addMarkView() {
		return ({ HTMLAttributes }) => {
			const dom = document.createElement('a');

			Object.entries(HTMLAttributes).forEach(([key, value]) => {
				if (value != null) dom.setAttribute(key, value);
			});

			dom.dataset.routerSlot = 'disabled';

			return { dom };
		};
	},

	addCommands() {
		return {
			ensureUmbLink: (attributes) => {
				return ({ editor, chain }) => {
					if (editor.isActive(this.name)) {
						return true;
					}
					return chain().setMark(this.name, attributes).setMeta('preventAutolink', true).run();
				};
			},
			setUmbLink: (attributes) => {
				return ({ chain }) => {
					return chain().setMark(this.name, attributes).setMeta('preventAutolink', true).run();
				};
			},
			unsetUmbLink: () => {
				return ({ chain }) => {
					return chain().unsetMark(this.name, { extendEmptyMarkRange: true }).setMeta('preventAutolink', true).run();
				};
			},
		};
	},
});

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		umbLink: {
			ensureUmbLink: (attributes: {
				type?: string | null;
				href: string;
				'data-anchor'?: string | null;
				target?: string | null;
				title?: string | null;
			}) => ReturnType;

			setUmbLink: (attributes: {
				type?: string | null;
				href: string;
				'data-anchor'?: string | null;
				target?: string | null;
				title?: string | null;
			}) => ReturnType;

			unsetUmbLink: () => ReturnType;
		};
	}
}
