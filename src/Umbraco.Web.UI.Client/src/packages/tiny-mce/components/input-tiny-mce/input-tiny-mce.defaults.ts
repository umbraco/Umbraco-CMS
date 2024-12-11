import { UMB_CONTEXT_REQUEST_EVENT_TYPE, type UmbContextRequestEvent } from '@umbraco-cms/backoffice/context-api';
import type { RawEditorOptions } from '@umbraco-cms/backoffice/external/tinymce';
import { UUIIconRequestEvent } from '@umbraco-cms/backoffice/external/uui';

//export const UMB_BLOCK_ENTRY_WEB_COMPONENTS_ABSOLUTE_PATH = '/umbraco/backoffice/packages/block/block-rte/index.js';
export const UMB_BLOCK_ENTRY_WEB_COMPONENTS_ABSOLUTE_PATH = '@umbraco-cms/backoffice/block-rte';

//we put these as extended elements because they get merged on top of the normal allowed elements by tiny mce
//so we don't have to specify all the normal elements again
export const defaultFallbackConfig: RawEditorOptions = {
	plugins: ['anchor', 'charmap', 'table', 'lists', 'advlist', 'autolink', 'directionality', 'searchreplace'],
	valid_elements:
		'+a[id|style|rel|data-id|data-udi|rev|charset|hreflang|dir|lang|tabindex|accesskey|type|name|href|target|title|class|onfocus|onblur|onclick|ondblclick|onmousedown|onmouseup|onmouseover|onmousemove|onmouseout|onkeypress|onkeydown|onkeyup],-strong/-b[class|style],-em/-i[class|style],-strike[class|style],-s[class|style],-u[class|style],#p[id|style|dir|class|align],-ol[class|reversed|start|style|type],-ul[class|style],-li[class|style],br[class],img[id|dir|lang|longdesc|usemap|style|class|src|onmouseover|onmouseout|border|alt=|title|hspace|vspace|width|height|align|umbracoorgwidth|umbracoorgheight|onresize|onresizestart|onresizeend|rel|data-id],-sub[style|class],-sup[style|class],-blockquote[dir|style|class],-table[border=0|cellspacing|cellpadding|width|height|class|align|summary|style|dir|id|lang|bgcolor|background|bordercolor],-tr[id|lang|dir|class|rowspan|width|height|align|valign|style|bgcolor|background|bordercolor],tbody[id|class],thead[id|class],tfoot[id|class],#td[id|lang|dir|class|colspan|rowspan|width|height|align|valign|style|bgcolor|background|bordercolor|scope],-th[id|lang|dir|class|colspan|rowspan|width|height|align|valign|style|scope],caption[id|lang|dir|class|style],-div[id|dir|class|align|style],-span[class|align|style],-pre[class|align|style],address[class|align|style],-h1[id|dir|class|align|style],-h2[id|dir|class|align|style],-h3[id|dir|class|align|style],-h4[id|dir|class|align|style],-h5[id|dir|class|align|style],-h6[id|style|dir|class|align|style],hr[class|style],small[class|style],dd[id|class|title|style|dir|lang],dl[id|class|title|style|dir|lang],dt[id|class|title|style|dir|lang],object[class|id|width|height|codebase|*],param[name|value|_value|class],embed[type|width|height|src|class|*],map[name|class],area[shape|coords|href|alt|target|class],bdo[class],button[class],iframe[*],figure,figcaption,cite,video[*],audio[*],picture[*],source[*],canvas[*],-code',
	invalid_elements: 'font',
	extended_valid_elements:
		'@[id|class|style],+umb-rte-block[!data-content-key],+umb-rte-block-inline[!data-content-key],-div[id|dir|class|align|style],ins[datetime|cite],-ul[class|style],-li[class|style],-h1[id|dir|class|align|style],-h2[id|dir|class|align|style],-h3[id|dir|class|align|style],-h4[id|dir|class|align|style],-h5[id|dir|class|align|style],-h6[id|style|dir|class|align],span[id|class|style|lang],figure,figcaption',
	custom_elements: 'umb-rte-block,~umb-rte-block-inline',
	toolbar: [
		'styles',
		'bold',
		'italic',
		'alignleft',
		'aligncenter',
		'alignright',
		'bullist',
		'numlist',
		'outdent',
		'indent',
		'link',
		'umbmediapicker',
		'umbembeddialog',
	],

	init_instance_callback: function (editor) {
		// The following code is the context api proxy. [NL]
		// It re-dispatches the context api request event to the origin target of this modal, in other words the element that initiated the modal. [NL]
		editor.dom.doc.addEventListener(UMB_CONTEXT_REQUEST_EVENT_TYPE, ((event: UmbContextRequestEvent) => {
			if (!editor.iframeElement) return;

			event.stopImmediatePropagation();
			editor.iframeElement.dispatchEvent(event.clone());
		}) as EventListener);

		// Proxy for retrieving icons from outside the iframe [NL]
		editor.dom.doc.addEventListener(UUIIconRequestEvent.ICON_REQUEST, ((event: UUIIconRequestEvent) => {
			if (!editor.iframeElement) return;

			const newEvent = new UUIIconRequestEvent(UUIIconRequestEvent.ICON_REQUEST, {
				detail: event.detail,
			});
			editor.iframeElement.dispatchEvent(newEvent);
			if (newEvent.icon !== null) {
				event.acceptRequest(newEvent.icon);
			}
		}) as EventListener);

		// Transfer our import-map to the iframe: [NL]
		const importMapTag = document.head.querySelector('script[type="importmap"]');
		if (importMapTag) {
			const importMap = document.createElement('script');
			importMap.type = 'importmap';
			importMap.text = importMapTag.innerHTML;
			editor.dom.doc.head.appendChild(importMap);
		}

		// Transfer our stylesheets to the iframe: [NL]
		const stylesheetTags = document.head.querySelectorAll<HTMLLinkElement>('link[rel="stylesheet"]');
		stylesheetTags.forEach((stylesheetTag) => {
			const stylesheet = document.createElement('link');
			stylesheet.rel = 'stylesheet';
			stylesheet.href = stylesheetTag.href;
			editor.dom.doc.head.appendChild(stylesheet);
		});

		editor.dom.doc.addEventListener('click', (e: MouseEvent) => {
			// If we try to open link in a new tab, then we want to skip skip:
			//if ((isWindows && e.ctrlKey) || (!isWindows && e.metaKey)) return;

			const composedPaths = 'composedPath' in e ? e.composedPath() : null;

			// Find the target by using the composed path to get the element through the shadow boundaries.
			// Notice the difference here compared to RouterSlots implementation [NL]
			const $anchor: HTMLAnchorElement =
				(composedPaths?.find(
					($elem) => $elem instanceof HTMLAnchorElement || ($elem as any).tagName === 'A',
				) as HTMLAnchorElement) ?? (e.target as HTMLAnchorElement);

			// Abort if the event is not about the anchor tag or the anchor tag has the attribute [data-router-slot]="disabled"
			if (
				$anchor == null ||
				!($anchor instanceof HTMLAnchorElement || ($anchor as any).tagName === 'A') ||
				$anchor.dataset['routerSlot'] === 'disabled'
			) {
				return;
			}

			// Abort if the anchor tag is not inside a block element
			const isInsideBlockElement =
				composedPaths?.some(
					($elem) => ($elem as any).tagName === 'UMB-RTE-BLOCK' || ($elem as any).tagName === 'UMB-RTE-BLOCK-INLINE',
				) ?? false;

			if (!isInsideBlockElement) {
				return;
			}

			// Remove the origin from the start of the HREF to get the path
			const path = $anchor.pathname + $anchor.search + $anchor.hash;

			// Prevent the default behavior
			e.preventDefault();

			// Change the history!
			window.history.pushState(null, '', path);
		});

		// Load backoffice JS so we can get the umb-rte-block component registered inside the iframe [NL]
		const script = document.createElement('script');
		script.type = 'text/javascript';
		script.setAttribute('type', 'module');

		script.text = `import "@umbraco-cms/backoffice/extension-registry";`;
		script.text = `import "${UMB_BLOCK_ENTRY_WEB_COMPONENTS_ABSOLUTE_PATH}";`;
		editor.dom.doc.head.appendChild(script);
	},

	style_formats: [
		{
			title: 'Headers',
			items: [
				{ title: 'Page header', block: 'h2' },
				{ title: 'Section header', block: 'h3' },
				{ title: 'Paragraph header', block: 'h4' },
			],
		},
		{
			title: 'Blocks',
			items: [{ title: 'Paragraph', block: 'p' }],
		},
		{
			title: 'Containers',
			items: [
				{ title: 'Quote', block: 'blockquote' },
				{ title: 'Code', block: 'code' },
			],
		},
	],
	/**
	 * @description The maximum image size in pixels that can be inserted into the editor.
	 * @remarks This is registered and used by the UmbMediaPicker plugin
	 */
	maxImageSize: 500,
};
