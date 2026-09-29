# Apply for a fishing rod licence

The start page is the service homepage. It shows the cookie banner, the live-demo notice, a start button, and a link to the component catalogue. You need to be 13 or over, and the example does not take payment. Do not enter information you want to keep private.

The journey is one thing per page, from a start button to a confirmation page. Every journey page uses the same header as the start page: the service name, English and Cymraeg, and the demonstration phase banner. Question pages also show a back link. The pages use shipped GOV.UK Frontend components only, inside the shared page shell (skip link, header, service navigation, phase banner “Example”, one `h1`, footer). Forms use `novalidate`, an error summary, field errors, and retained values. Check answers uses a summary list. Confirmation uses a panel and a reference number. Pages do not show breadcrumbs and a back link together.

Personal-data pages use the `sensitive-document` cache kind. The draft is stored in the session cookie `fishing-example`.

```text
/apply                  Start now
/apply/length           How long do you need the licence for? (1 day, 8 days, 12 months)
/apply/name             Full name
/apply/date-of-birth    Date of birth (at least 13 years old)
/apply/country          England, Wales, or Scotland
/apply/email            Email address
/apply/check            Check your answers
/apply/confirmation     Application complete, reference FR########
```

A later page redirects to the first incomplete earlier step, so a Check answers “Change” link still opens a page that already has an answer. Confirmation without a reference returns to `/apply`.

`/` redirects to `/apply`, so the site opens on this start page. The catalogue stays at `/components`.
