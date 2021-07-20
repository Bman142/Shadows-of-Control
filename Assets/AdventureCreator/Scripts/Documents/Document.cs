﻿/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2021
 *	
 *	"Document.cs"
 * 
 *	Stores data for a document, which can be viewed/read in a Menu
 * 
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;

namespace AC
{

	/**
	 * Stores data for a document, which can be viewed/read in a Menu
	 */
	[System.Serializable]
	public class Document : ITranslatable
	{

		#region Variables

		/** A unique identifier */
		public int ID;
		/** The title */
		public string title;
		/** The translation ID number of the title, as generated by SpeechManager */
		public int titleLineID = -1;
		/** If True, the Document will be re-opened at the same page that it was closed at */
		public bool rememberLastOpenPage = false;
		/** If True, the Document will be in the Player's collection when the game begins */
		public bool carryOnStart = false;
		/** A Texture2D associated with the Document. */
		public Texture2D texture;
		/** A List of JournalPages that make up the contents of the Document */
		public List<JournalPage> pages = new List<JournalPage>();
		/** The ID number of the document's InvBin category, as defined in InventoryManager */
		public int binID = 0;

		#endregion


		#region Constructors

		/**
		 * <summary>The default Constructor.</summary>
		 * <param name = "idArray">An array of already-used ID numbers, so that a unique ID number can be assigned</param>
		 */
		public Document (int[] idArray)
		{
			title = string.Empty;
			titleLineID = -1;
			rememberLastOpenPage = false;
			texture = null;
			pages = new List<JournalPage>();
			carryOnStart = false;
			binID = 0;

			ID = 0;
			// Update id based on array
			foreach (int _id in idArray)
			{
				if (ID == _id)
					ID ++;
			}
		}


		/**
		 * <summary>A Constructor in which the ID is explicitly set.</summary>
		 * <param name = "_ID">The ID number to assign</param>
		 */
		public Document (int _ID)
		{
			title = string.Empty;
			titleLineID = -1;
			rememberLastOpenPage = false;
			texture = null;
			pages = new List<JournalPage>();
			carryOnStart = false;
			binID = 0;

			ID = _ID;
		}

		#endregion


		#region PublicFunctions

		/**
		 * <summary>Gets the text of a given page</summary>
		 * <param name = "pageIndex">The index of the page</param>
		 * <param name = "languageNumber">The index number of the language to get the text in, where 0 = original language and >0 = translations</param>
		 * <returns>The page text</returns>
		 */
		public string GetPageText (int pageIndex, int languageNumber = 0)
		{
			if (pages != null && pageIndex < pages.Count && pageIndex > 0)
			{
				JournalPage page = pages[pageIndex];
				return KickStarter.runtimeLanguages.GetTranslation (page.text, page.lineID, languageNumber, GetTranslationType (0));
			}
			return string.Empty;
		}


		/**
		 * <summary>Gets the Document's title</summary>
		 * <param name = "languageNumber">The index number of the language to get the text in, where 0 = original language and >0 = translations</param>
		 * <returns>The Document's title</returns>
		 */
		public string GetTitleText (int languageNumber = 0)
		{
			return KickStarter.runtimeLanguages.GetTranslation (title, titleLineID, languageNumber, GetTranslationType (0));
		}

		#endregion


		#region GetSet

		/**
		 * The Document's title.  This will set the title to '(Untitled)' if empty.
		 */
		public string Title
		{
			get
			{
				if (string.IsNullOrEmpty (title))
				{
					title = "(Untitled)";
				}
				return title;
			}
		}

		#endregion


		#if UNITY_EDITOR

		protected int sidePage;
		protected int selectedPage;
		protected Vector2 scrollPos;
		protected bool showPageGUI = true;


		public void ClearIDs ()
		{
			titleLineID = -1;
			foreach (JournalPage page in pages)
			{
				page.lineID = -1;
			}
		}


		protected int GetBinSlot (List<InvBin> bins, int _id)
		{
			int i = 0;
			foreach (InvBin bin in bins)
			{
				if (bin.id == _id)
				{
					return i;
				}
				i++;
			}
			
			return 0;
		}


		public void ShowGUI (string apiPrefix, List<InvBin> bins)
		{
			title = CustomGUILayout.TextField ("Title:", title, apiPrefix + ".title");
			if (titleLineID > -1)
			{
				EditorGUILayout.LabelField ("Speech Manager ID:", titleLineID.ToString ());
			}

			texture = (Texture2D) CustomGUILayout.ObjectField <Texture2D> ("Texture:", texture, false, apiPrefix + ".texture", "A Texture2D associated with the Document");
			carryOnStart = CustomGUILayout.Toggle ("Carry on start?", carryOnStart, apiPrefix + ".carryOnStart", "If True, the Document will be in the Player's collection when the game begins");
			rememberLastOpenPage = CustomGUILayout.Toggle ("Remember last-open page?", rememberLastOpenPage, ".rememberLastOpenPage", "If True, the Document will be re-opened at the same page that it was closed at");

			//
			if (bins.Count > 0)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField (new GUIContent ("Category:", "The category that the document belongs to"), GUILayout.Width (146f));
				int binNumber = GetBinSlot (bins, binID);

				List<string> binList = new List<string>();
				foreach (InvBin bin in bins)
				{
					binList.Add (bin.EditorLabel);
				}

				binNumber = CustomGUILayout.Popup (binNumber, binList.ToArray(), apiPrefix + ".binID");
				binID = bins[binNumber].id;
				EditorGUILayout.EndHorizontal ();
			}
			//

			EditorGUILayout.Space ();

			EditorGUILayout.LabelField ("Document pages:");

			if (pages == null || pages.Count == 0)
			{
				pages.Clear ();
				pages.Add (new JournalPage ());
			}

			scrollPos = EditorGUILayout.BeginScrollView (scrollPos, GUILayout.Height (Mathf.Min (pages.Count * 21, 185f)+5));
			for (int i=0; i<pages.Count; i++)
			{
				EditorGUILayout.BeginHorizontal ();

				if (GUILayout.Toggle (selectedPage == i, "Page #" + i.ToString (), "Button"))
				{
					if (selectedPage != i)
					{
						selectedPage = i;
						EditorGUIUtility.editingTextField = false;
					}
				}

				if (GUILayout.Button ("", CustomStyles.IconCog))
				{
					sidePage = i;
					EditorGUIUtility.editingTextField = false;
					SidePageMenu ();
				}
				EditorGUILayout.EndHorizontal ();
			}
			EditorGUILayout.EndScrollView ();

			if (GUILayout.Button ("Create new page"))
			{
				Undo.RecordObject (KickStarter.inventoryManager, "Add Document page");
				pages.Add (new JournalPage ());

				if (pages.Count == 1)
				{
					selectedPage = 0;
					EditorGUIUtility.editingTextField = false;
				}
			}
			CustomGUILayout.EndVertical ();

			EditorGUILayout.Space ();

			if (selectedPage >= 0 && pages.Count > selectedPage)
			{
				CustomGUILayout.BeginVertical ();
				showPageGUI = CustomGUILayout.ToggleHeader (showPageGUI, "Document page #" + selectedPage);
				if (showPageGUI)
				{
					CustomGUILayout.LabelField ("Page text:", apiPrefix + ".pages[" + selectedPage + "].text");
					EditorStyles.textField.wordWrap = true;
					pages[selectedPage].text = EditorGUILayout.TextArea (pages[selectedPage].text, GUILayout.MaxWidth (400f));
				}
				CustomGUILayout.EndVertical ();
			}
		}


		protected void SidePageMenu ()
		{
			GenericMenu menu = new GenericMenu ();

			menu.AddItem (new GUIContent ("Insert after"), false, PageCallback, "Insert after");
			if (pages.Count > 1)
			{
				menu.AddItem (new GUIContent ("Delete"), false, PageCallback, "Delete");
			}

			if (sidePage > 0 || sidePage < pages.Count-1)
			{
				menu.AddSeparator ("");
				if (sidePage > 0)
				{
					menu.AddItem (new GUIContent ("Re-arrange/Move to top"), false, PageCallback, "Move to top");
					menu.AddItem (new GUIContent ("Re-arrange/Move up"), false, PageCallback, "Move up");
				}
				if (sidePage < pages.Count-1)
				{
					menu.AddItem (new GUIContent ("Re-arrange/Move down"), false, PageCallback, "Move down");
					menu.AddItem (new GUIContent ("Re-arrange/Move to bottom"), false, PageCallback, "Move to bottom");
				}
			}
			
			menu.ShowAsContext ();
		}


		protected void PageCallback (object obj)
		{
			if (sidePage >= 0)
			{
				switch (obj.ToString ())
				{
					case "Insert after":
						Undo.RecordObject (KickStarter.inventoryManager, "Insert Document page");
						pages.Insert (sidePage+1, new JournalPage ());
						break;
						
					case "Delete":
						Undo.RecordObject (KickStarter.inventoryManager, "Delete Document page");
						if (sidePage == selectedPage)
						{
							selectedPage = -1;
						}
						pages.RemoveAt (sidePage);
						break;
						
					case "Move up":
						Undo.RecordObject (KickStarter.inventoryManager, "Move page up");
						if (sidePage == selectedPage)
						{
							selectedPage --;
						}
						SwapPages (sidePage, sidePage-1);
						break;
						
					case "Move down":
						Undo.RecordObject (KickStarter.inventoryManager, "Move page down");
						if (sidePage == selectedPage)
						{
							selectedPage ++;
						}
						SwapPages (sidePage, sidePage+1);
						break;

					case "Move to top":
						Undo.RecordObject (KickStarter.inventoryManager, "Move page to top");
						if (sidePage == selectedPage)
						{
							selectedPage --;
						}
						MovePageToTop (sidePage);
						break;
					
					case "Move to bottom":
						Undo.RecordObject (KickStarter.inventoryManager, "Move page to bottom");
						if (sidePage == selectedPage)
						{
							selectedPage ++;
						}
						MovePageToBottom (sidePage);
						break;
				}
			}
			
			sidePage = -1;
		}


		protected void MovePageToTop (int a1)
		{
			JournalPage tempPage = pages[a1];
			pages.Insert (0, tempPage);
			pages.RemoveAt (a1+1);
		}


		protected void MovePageToBottom (int a1)
		{
			JournalPage tempPage = pages[a1];
			pages.Add (tempPage);
			pages.RemoveAt (a1);
		}
		

		protected void SwapPages (int a1, int a2)
		{
			JournalPage tempPage = pages[a1];
			pages[a1] = pages[a2];
			pages[a2] = tempPage;
		}

		#endif


		#region ITranslatable

		public string GetTranslatableString (int index)
		{
			if (index == 0)
			{
				return Title;
			}
			else
			{
				return pages[index-1].text;
			}
		}


		public int GetTranslationID (int index)
		{
			if (index == 0)
			{
				return titleLineID;
			}
			else
			{
				return pages[index-1].lineID;
			}
		}


		public AC_TextType GetTranslationType (int index)
		{
			return AC_TextType.Document;
		}


		#if UNITY_EDITOR

		public void UpdateTranslatableString (int index, string updatedText)
		{
			if (index == 0)
			{
				title = updatedText;
			}
			else if ((index-1) < pages.Count)
			{
				pages[index-1].text = updatedText;
			}
		}


		public int GetNumTranslatables ()
		{
			if (pages != null) return pages.Count + 1;
			return 1;
		}


		public bool HasExistingTranslation (int index)
		{
			if (index == 0)
			{
				return titleLineID > -1;
			}
			else
			{
				return (pages[index-1].lineID > -1);
			}
		}



		public void SetTranslationID (int index, int _lineID)
		{
			if (index == 0)
			{
				titleLineID = _lineID;
			}
			else
			{
				pages[index-1].lineID = _lineID;
			}
		}


		public string GetOwner (int index)
		{
			return string.Empty;
		}


		public bool OwnerIsPlayer (int index)
		{
			return false;
		}


		public bool CanTranslate (int index)
		{
			if (index == 0)
			{
				return !string.IsNullOrEmpty (title);
			}
			return (!string.IsNullOrEmpty (pages[index-1].text));
		}

		#endif

		#endregion

	}

}